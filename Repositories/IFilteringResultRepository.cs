// Repositories/IFilteringResultRepository.cs
using ms_filtering_ia.Models;

namespace ms_filtering_ia.Repositories;

public interface IFilteringResultRepository
{
    Task AddAsync(FilteringResult result);
    Task AddRangeAsync(IEnumerable<FilteringResult> results); 
    Task<List<FilteringResult>> GetAllAsync();
    Task<FilteringResult?> GetByIdAsync(int id);
    Task<HashSet<string>> GetAcceptedCvPathsAsync();
    Task SaveChangesAsync();
}