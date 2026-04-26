using Microsoft.EntityFrameworkCore;
using ms_filtering_ia.Data;
using ms_filtering_ia.Models;

namespace ms_filtering_ia.Repositories;

public sealed class FilteringResultRepository : IFilteringResultRepository
{
    private readonly AppDbContext _db;

    public FilteringResultRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(FilteringResult result)
        => await _db.FilteringResults.AddAsync(result);

    public async Task<List<FilteringResult>> GetAllAsync()
        => await _db.FilteringResults.ToListAsync();

    public async Task<FilteringResult?> GetByIdAsync(int id)
        => await _db.FilteringResults.FirstOrDefaultAsync(r => r.Id == id);

    /// <summary>
    /// Retourne les chemins MinIO de tous les CVs dont un candidat a été accepté.
    /// Utilisé pour les exclure du prochain filtrage.
    /// </summary>
    public async Task<HashSet<string>> GetAcceptedCvPathsAsync()
    {
        var paths = await _db.FilteringResults
            .Where(r => r.CvStatus == "accepted")
            .Select(r => r.CvMinioPath)
            .Distinct()
            .ToListAsync();

        return paths.ToHashSet();
    }

    public async Task SaveChangesAsync()
        => await _db.SaveChangesAsync();
}