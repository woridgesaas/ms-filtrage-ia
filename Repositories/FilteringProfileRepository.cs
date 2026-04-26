using Microsoft.EntityFrameworkCore;
using ms_filtering_ia.Data;
using ms_filtering_ia.Models;

namespace ms_filtering_ia.Repositories;

public sealed class FilteringProfileRepository : IFilteringProfileRepository
{
    private readonly AppDbContext _db;

    public FilteringProfileRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<FilteringProfile> CreateAsync(FilteringProfile profile)
    {
        await _db.FilteringProfiles.AddAsync(profile);
        return profile;
    }

    public async Task<User?> GetUserByCinAsync(string cin)
        => await _db.Users.FirstOrDefaultAsync(u => u.Cin == cin);

    public async Task<CriteriaType?> GetOrCreateCriteriaAsync(string name)
    {
        var existing = await _db.CriteriaTypes
            .FirstOrDefaultAsync(c => c.CriteriaName == name);

        if (existing is not null)
            return existing;

        var created = new CriteriaType { CriteriaName = name };
        await _db.CriteriaTypes.AddAsync(created);
        return created;
    }

    public async Task SaveChangesAsync()
        => await _db.SaveChangesAsync();
}