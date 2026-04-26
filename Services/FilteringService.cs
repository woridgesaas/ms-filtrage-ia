using System.Collections.Concurrent;
using ms_filtering_ia.DTOs;
using ms_filtering_ia.Mappers;
using ms_filtering_ia.Messaging.Events;
using ms_filtering_ia.Messaging.Publisher;
using ms_filtering_ia.Models;
using ms_filtering_ia.Repositories;

namespace ms_filtering_ia.Services;

public sealed class FilteringService : IFilteringService
{
    private readonly ICvTextExtractorService _cvExtractor;
    private readonly IPythonApiService _pythonApi;
    private readonly IMinioService _minio;
    private readonly IFilteringResultRepository _repository;
    private readonly IRabbitMQPublisher _publisher;
    private readonly ILogger<FilteringService> _logger;

    // Limite les appels concurrents vers Python/Ollama
    private const int MaxParallelPythonCalls = 1;

    public FilteringService(
        ICvTextExtractorService cvExtractor,
        IPythonApiService pythonApi,
        IMinioService minio,
        IFilteringResultRepository repository,
        IRabbitMQPublisher publisher,
        ILogger<FilteringService> logger)
    {
        _cvExtractor = cvExtractor;
        _pythonApi = pythonApi;
        _minio = minio;
        _repository = repository;
        _publisher = publisher;
        _logger = logger;
    }

    // ── Pipeline "Filtrer par IA" ─────────────────────────────────────────
    // Phase 1 : extraction texte séquentielle (OCR non thread-safe)
    // Phase 2 : index + extract en parallèle (goulot Python/Ollama)
    // Phase 3 : match global → croisement → sauvegarde batch
    // ─────────────────────────────────────────────────────────────────────
    public async Task<List<FilteringResultResponse>> AnalyzeAllCvsAsync(AnalyzeCvsRequest request)
    {
        var allKeys = await _minio.ListCvsAsync();
        var acceptedPaths = await _repository.GetAcceptedCvPathsAsync();
        var toProcess = allKeys.Where(k => !acceptedPaths.Contains(k)).ToList();

        _logger.LogInformation(
            "[FilteringService] Total={T} | Exclus={E} | À traiter={P}",
            allKeys.Count, allKeys.Count - toProcess.Count, toProcess.Count);

        if (toProcess.Count == 0)
            return new List<FilteringResultResponse>();

        // ── Phase 1 : extraction texte (séquentielle — OCR non thread-safe) ───
        var cvTexts = new Dictionary<string, string>(toProcess.Count);

        foreach (var cvKey in toProcess)
        {
            try
            {
                cvTexts[cvKey] = await _cvExtractor.ExtractAsync(cvKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[FilteringService] Extraction impossible pour {Key} — ignoré.", cvKey);
            }
        }

        if (cvTexts.Count == 0)
        {
            _logger.LogWarning("[FilteringService] Aucun CV extractible.");
            return new List<FilteringResultResponse>();
        }

        // ── Phase 2 : index + extract en parallèle (appels Python) ────────────
        var extractions = new ConcurrentDictionary<string, (string MinioPath, ExtractionResponse Ext)>();
        var semaphore = new SemaphoreSlim(MaxParallelPythonCalls);

        var indexTasks = cvTexts.Select(async kvp =>
        {
            var (cvKey, cvText) = (kvp.Key, kvp.Value);
            await semaphore.WaitAsync();
            try
            {
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
                    "[FilteringService] Erreur Python sur {Key} — ignoré.", cvKey);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(indexTasks);

        if (extractions.IsEmpty)
        {
            _logger.LogWarning("[FilteringService] Aucun CV indexé avec succès.");
            return new List<FilteringResultResponse>();
        }

        // ── Phase 3 : match global ─────────────────────────────────────────────
        var matchResp = await _pythonApi.MatchAsync(BuildMatchRequest(request));

        _logger.LogInformation(
            "[FilteringService] /match terminé : {Count} candidats retournés.",
            matchResp.Candidates.Count);

        // ── Phase 4 : croisement + sauvegarde batch (fix N+1) ─────────────────
        var toSave = new List<FilteringResult>();
        var dtoData = new List<(FilteringResult Entity, ExtractionResponse Ext, CandidateMatch Match)>();

        foreach (var candidate in matchResp.Candidates)
        {
            if (!extractions.TryGetValue(candidate.CvId, out var entry))
            {
                _logger.LogWarning(
                    "[FilteringService] cv_id={Id} retourné par /match mais non indexé localement.",
                    candidate.CvId);
                continue;
            }

            var (minioPath, ext) = entry;
            var entity = BuildResult(candidate, ext, minioPath, request.FilteringProfileId);
            toSave.Add(entity);
            dtoData.Add((entity, ext, candidate));
        }

        if (toSave.Count > 0)
        {
            await _repository.AddRangeAsync(toSave);
            await _repository.SaveChangesAsync();
        }

        var results = dtoData
            .Select(d => FilteringResultMapper.ToDto(d.Entity, d.Ext.Data, d.Match))
            .ToList();

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

    public async Task<string> GetCvPresignedUrlAsync(string fileName)
        => await _minio.GetPresignedUrlAsync(fileName);

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
    private static MatchingRequest BuildMatchRequest(AnalyzeCvsRequest r) => new()
    {
        JobOffer = new JobOfferDto
        {
            JobTitle = r.OfferTitle,
            RequiredSkills = r.TechSkills,
            RequiredTools = r.Tools,
            RequiredExperienceYears = r.MinExperience,
            RequiredDegree = r.Degree,
            RequiredLanguages = string.IsNullOrEmpty(r.Language)
                ? new List<LangRequirement>()
                : new List<LangRequirement>
                  { new() { Lang = r.Language, Level = r.LanguageLevel } },
            Location = r.Location,
            Certifications = r.Certifications,
            SoftSkills = r.SoftSkills,
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

        return new FilteringResult //hadchi khss ykon f mapper ta lmb3d 
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
            ExtractedSoftSkills = string.Join(",", data.SoftSkills),
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