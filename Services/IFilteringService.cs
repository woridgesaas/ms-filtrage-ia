using ms_filtering_ia.DTOs;

namespace ms_filtering_ia.Services;

public interface IFilteringService
{
    Task<List<FilteringResultResponse>> AnalyzeAllCvsAsync(AnalyzeCvsRequest request);
    Task<List<FilteringResultResponse>> GetAllResultsAsync();
    Task<AcceptCandidateResponse>       AcceptCandidateAsync(int id, int offerId, string userCin);
    Task<RejectCandidateResponse>       RejectCandidateAsync(int id);
    
    /// <summary>Retourne une URL présignée MinIO pour visualiser un CV depuis Angular.</summary>
    Task<string> GetCvPresignedUrlAsync(string fileName);
}