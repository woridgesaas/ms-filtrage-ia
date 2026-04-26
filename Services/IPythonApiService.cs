using ms_filtering_ia.DTOs;

namespace ms_filtering_ia.Services;

public interface IPythonApiService
{
    Task<IndexResponse>      IndexAsync(IndexRequest request);
    Task<ExtractionResponse> ExtractAsync(string cvId, string cvText);
    Task<MatchingResponse>   MatchAsync(MatchingRequest request);
}