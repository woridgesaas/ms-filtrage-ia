using ms_filtering_ia.DTOs;
using ms_filtering_ia.Mappers;
using ms_filtering_ia.Messaging.Events;
using ms_filtering_ia.Messaging.Publisher;
using ms_filtering_ia.Models;
using ms_filtering_ia.Repositories;

namespace ms_filtering_ia.Services;

public sealed class FilteringService : IFilteringService
{
    private readonly IMinioService _minio;
    private readonly IPdfExtractorService _pdfExtractor;
    private readonly IOcrService _ocr;
    private readonly IPythonApiService _pythonApi;
    private readonly IFilteringResultRepository _repository;
    private readonly IRabbitMQPublisher _publisher;
    private readonly ILogger<FilteringService> _logger;

    private const int MinTextLength = 50;

    public FilteringService(
        IMinioService minio,
        IPdfExtractorService pdfExtractor,
        IOcrService ocr,
        IPythonApiService pythonApi,
        IFilteringResultRepository repository,
        IRabbitMQPublisher publisher,
        ILogger<FilteringService> logger)
    {
        _minio = minio;
        _pdfExtractor = pdfExtractor;
        _ocr = ocr;
        _pythonApi = pythonApi;
        _repository = repository;
        _publisher = publisher;
        _logger = logger;
    }

    // ── Pipeline "Filtrer par IA" ─────────────────────────────────────────
    // Étapes :
    //   1. Index + Extract pour chaque CV MinIO
    //   2. Un seul appel /match avec le job_offer → Python score tous les CVs
    //   3. On croise les résultats pour sauvegarder en DB
    // ─────────────────────────────────────────────────────────────────────
    public async Task<List<FilteringResultResponse>> AnalyzeAllCvsAsync(OfferForm offer)
    {
        var allKeys = await _minio.ListCvsAsync();
        var acceptedPaths = await _repository.GetAcceptedCvPathsAsync();
        var toProcess = allKeys.Where(k => !acceptedPaths.Contains(k)).ToList();

        _logger.LogInformation(
            "[FilteringService] Total={T} | Exclus={E} | À traiter={P}",
            allKeys.Count, allKeys.Count - toProcess.Count, toProcess.Count);

        if (toProcess.Count == 0)
            return new List<FilteringResultResponse>();

        // ── Étape 1 : Index + Extract pour chaque CV ──────────────────────
        var extractions = new Dictionary<string, (string MinioPath, ExtractionResponse Ext)>();

        foreach (var cvKey in toProcess)
        {
            try
            {
                var cvText = await ExtractCvTextAsync(cvKey);
                var indexResp = await _pythonApi.IndexAsync(new IndexRequest
                {
                    CvText = cvText,
                    Metadata = new IndexMetadata { Filename = cvKey }
                });
                var extractResp = await _pythonApi.ExtractAsync(indexResp.CvId, cvText);

                extractions[indexResp.CvId] = (cvKey, extractResp);

                _logger.LogInformation(
                    "[FilteringService] Indexé+Extrait : {Key} → cv_id={Id}",
                    cvKey, indexResp.CvId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[FilteringService] Erreur index/extract sur {Key} — ignoré.", cvKey);
            }
        }

        if (extractions.Count == 0)
        {
            _logger.LogWarning("[FilteringService] Aucun CV indexé avec succès.");
            return new List<FilteringResultResponse>();
        }

        // ── Étape 2 : Un seul appel /match ────────────────────────────────
        var matchRequest = BuildMatchRequest(offer);
        var matchResp = await _pythonApi.MatchAsync(matchRequest);

        _logger.LogInformation(
            "[FilteringService] /match terminé : {Count} candidats retournés.",
            matchResp.Candidates.Count);

        // ── Étape 3 : Croisement + sauvegarde ─────────────────────────────
        var results = new List<FilteringResultResponse>();

        foreach (var candidate in matchResp.Candidates)
        {
            if (!extractions.TryGetValue(candidate.CvId, out var entry))
            {
                _logger.LogWarning(
                    "[FilteringService] cv_id={Id} retourné par /match mais non trouvé localement.",
                    candidate.CvId);
                continue;
            }

            var (minioPath, ext) = entry;
            var result = BuildResult(candidate, ext, minioPath, offer.FilteringProfileId);

            try
            {
                await _repository.AddAsync(result);
                await _repository.SaveChangesAsync();
                results.Add(FilteringResultMapper.ToDto(result, ext.Data, candidate));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[FilteringService] Erreur sauvegarde cv_id={Id}.", candidate.CvId);
            }
        }

        _logger.LogInformation(
            "[FilteringService] Terminé : {Done}/{Total} CVs sauvegardés.",
            results.Count, toProcess.Count);

        return results;
    }

    public async Task<List<FilteringResultResponse>> GetAllResultsAsync()
    {
        var results = await _repository.GetAllAsync();
        return FilteringResultMapper.ToDtoList(results);
    }

    // ── Actions RH ────────────────────────────────────────────────────────
    public async Task<AcceptCandidateResponse> AcceptCandidateAsync(
        int id, int offerId, string userCin)
    {
        var result = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"FilteringResult {id} introuvable.");

        result.Decision = "Accepté";
        result.CvStatus = "accepted";
        await _repository.SaveChangesAsync();

        await _publisher.PublishCandidateAcceptedAsync(new CandidateAcceptedEvent
        {
            CandidateName = result.CandidateName,
            CandidateEmail = result.CandidateEmail,
            CandidatePhone = result.CandidatePhone,
            Location = result.CandidateLocation,
            AiScore = result.GlobalScore,
            MainDomain = result.MainDomain,
            CvMinioUrl = result.CvMinioPath,
            OfferId = offerId,
            AcceptedAt = DateTime.UtcNow,
            UserCin = userCin
        });

        _logger.LogInformation("[FilteringService] Candidat {Id} accepté.", id);

        return new AcceptCandidateResponse
        {
            Id = result.Id,
            CandidateName = result.CandidateName,
            Decision = result.Decision,
            Message = "Candidat accepté et transmis à candidate-management."
        };
    }

    public async Task<RejectCandidateResponse> RejectCandidateAsync(int id)
    {
        var result = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"FilteringResult {id} introuvable.");

        result.Decision = "Refusé";
        result.CvStatus = "rejected";
        await _repository.SaveChangesAsync();

        _logger.LogInformation("[FilteringService] Candidat {Id} refusé.", id);

        return new RejectCandidateResponse
        {
            Id = result.Id,
            CandidateName = result.CandidateName,
            Decision = result.Decision
        };
    }

    // ── Helpers privés ────────────────────────────────────────────────────

    private async Task<string> ExtractCvTextAsync(string cvMinioPath)
    {
        var pdfBytes = await _minio.GetCvBytesAsync(cvMinioPath);

        using var stream1 = new MemoryStream(pdfBytes);
        var text = _pdfExtractor.ExtractText(stream1);

        if (!string.IsNullOrWhiteSpace(text) && text.Length >= MinTextLength)
            return text;

        _logger.LogWarning(
            "[FilteringService] PDF scanné ({Len} chars) → OCR : {Path}",
            text.Length, cvMinioPath);

        using var stream2 = new MemoryStream(pdfBytes);
        text = _ocr.ExtractTextFromImages(stream2);

        if (string.IsNullOrWhiteSpace(text) || text.Length < 20)
            throw new InvalidOperationException($"CV illisible même après OCR : {cvMinioPath}");

        return text;
    }

    private static MatchingRequest BuildMatchRequest(OfferForm offer) => new()
    {
        JobOffer = new JobOfferDto
        {
            JobTitle = offer.OfferTitle,
            RequiredSkills = offer.TechSkills,
            RequiredTools = offer.Tools,
            RequiredExperienceYears = offer.MinExperience,
            RequiredDegree = offer.Degree,
            RequiredLanguages = string.IsNullOrEmpty(offer.Language)
                ? new List<LangRequirement>()
                : new List<LangRequirement>
                {
                    new() { Lang = offer.Language, Level = offer.LanguageLevel }
                },
            Location = offer.Location,
            Certifications = offer.Certifications,
            SoftSkills = offer.SoftSkills,
        },
        TopK = 20,
        SimilarityThreshold = 0.3,
        MaxResults = 50,
    };

    private static FilteringResult BuildResult(
    CandidateMatch candidate,
    ExtractionResponse ext,
    string minioPath,
    int filteringProfileId)
    {
        var data = ext.Data;
        var name = $"{data.PersonalInfo.FirstName} {data.PersonalInfo.LastName}".Trim();
        var techSkills = data.TechnicalSkills.Concat(data.Tools).Distinct();

        return new FilteringResult
        {
            CvId = candidate.CvId,
            CandidateName = string.IsNullOrWhiteSpace(name) ? candidate.Name : name,
            CandidateEmail = data.PersonalInfo.Email,
            CandidatePhone = data.PersonalInfo.Phone,
            CandidateLocation = data.PersonalInfo.Location,
            CvMinioPath = minioPath,
            GlobalScore = candidate.MatchScore,
            MainDomain = data.MainDomain,
            SeniorityLevel = data.SeniorityLevel,
            ExtractedSkills = string.Join(",", techSkills),
            ExtractedSoftSkills = string.Join(",", data.SoftSkills),  // ← séparé
            MissingSkills = string.Join(",", candidate.MissingSkills),
            Recommendation = candidate.Recommendation,
            ExperienceYears = data.TotalExperienceYears,
            Decision = DetermineDecision(candidate.MatchScore),
            CvStatus = "pending",
            FilteringProfileId = filteringProfileId == 0 ? null : filteringProfileId,
        };
    }
    private static string DetermineDecision(double score) => score switch
    {
        >= 80 => "Accepté",
        >= 60 => "En attente",
        _ => "Refusé",
    };
}