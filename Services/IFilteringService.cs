using ms_filtering_ia.DTOs;

namespace ms_filtering_ia.Services;

public interface IFilteringService
{
    Task<List<FilteringResultResponse>> AnalyzeAllCvsAsync(OfferForm offer);
    Task<List<FilteringResultResponse>> GetAllResultsAsync();
    Task<AcceptCandidateResponse>       AcceptCandidateAsync(int id, int offerId, string userCin);
    Task<RejectCandidateResponse>       RejectCandidateAsync(int id);
}