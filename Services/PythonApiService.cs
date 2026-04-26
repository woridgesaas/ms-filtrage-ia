using System.Net.Http.Json;
using System.Text.Json;
using ms_filtering_ia.DTOs;

namespace ms_filtering_ia.Services;

public sealed class PythonApiService : IPythonApiService
{
    private readonly HttpClient                _httpClient;
    private readonly ILogger<PythonApiService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
    };

    public PythonApiService(HttpClient httpClient, ILogger<PythonApiService> logger)
    {
        _httpClient = httpClient;
        _logger     = logger;
    }

    public async Task<IndexResponse> IndexAsync(IndexRequest request)
    {
        _logger.LogInformation("[/index] {Filename}", request.Metadata.Filename);
        var result = await PostAsync<IndexResponse>("/api/v1/index", request, "/index");
        _logger.LogInformation("[/index] cv_id={CvId} | status={Status}",
            result.CvId, result.Status);
        return result;
    }

    public async Task<ExtractionResponse> ExtractAsync(string cvId, string cvText)
    {
        _logger.LogInformation("[/extract] cv_id={CvId}", cvId);
        var result = await PostAsync<ExtractionResponse>(
            "/api/v1/extract",
            new { cv_id = cvId, cv_text = cvText },
            "/extract");
        _logger.LogInformation(
            "[/extract] {Skills} skills | {Exp} ans | {Seniority} | {Domain}",
            result.Data.TechnicalSkills.Count + result.Data.Tools.Count,
            result.Data.TotalExperienceYears,
            result.Data.SeniorityLevel,
            result.Data.MainDomain);
        return result;
    }

    public async Task<MatchingResponse> MatchAsync(MatchingRequest request)
    {
        _logger.LogInformation("[/match] offre={Title} | top_k={TopK}",
            request.JobOffer.JobTitle, request.TopK);
        var result = await PostAsync<MatchingResponse>("/api/v1/match", request, "/match");
        _logger.LogInformation("[/match] {Count} candidats scorés.", result.Candidates.Count);
        return result;
    }

    // ── Helper commun — élimine la duplication log+ensure+deserialize ─────
    private async Task<TResponse> PostAsync<TResponse>(
        string endpoint, object payload, string logLabel)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, payload, JsonOptions);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            _logger.LogError("[{Label}] {Status} — {Body}",
                logLabel, (int)response.StatusCode, err);
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions)
               ?? throw new InvalidOperationException($"{endpoint} a retourné null");
    }
}