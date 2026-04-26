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

        var response = await _httpClient.PostAsJsonAsync("/api/v1/index", request, JsonOptions);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            _logger.LogError("[/index] {Status} — {Body}", (int)response.StatusCode, err);
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IndexResponse>(JsonOptions)
                     ?? throw new InvalidOperationException("/index a retourné null");

        _logger.LogInformation("[/index] cv_id={CvId} | status={Status}",
            result.CvId, result.Status);
        return result;
    }

    public async Task<ExtractionResponse> ExtractAsync(string cvId, string cvText)
    {
        _logger.LogInformation("[/extract] cv_id={CvId}", cvId);

        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/extract",
            new { cv_id = cvId, cv_text = cvText },
            JsonOptions);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            _logger.LogError("[/extract] {Status} — {Body}", (int)response.StatusCode, err);
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ExtractionResponse>(JsonOptions)
                     ?? throw new InvalidOperationException("/extract a retourné null");

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

        var response = await _httpClient.PostAsJsonAsync("/api/v1/match", request, JsonOptions);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            _logger.LogError("[/match] {Status} — {Body}", (int)response.StatusCode, err);
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MatchingResponse>(JsonOptions)
                     ?? throw new InvalidOperationException("/match a retourné null");

        _logger.LogInformation("[/match] {Count} candidats scorés.", result.Candidates.Count);
        return result;
    }
}