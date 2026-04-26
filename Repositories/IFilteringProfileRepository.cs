using ms_filtering_ia.Models;

namespace ms_filtering_ia.Repositories;

public interface IFilteringProfileRepository
{
    Task<FilteringProfile> CreateAsync(FilteringProfile profile);
    Task<User?> GetUserByCinAsync(string cin);
    Task<CriteriaType?> GetOrCreateCriteriaAsync(string name);
    Task SaveChangesAsync();
}