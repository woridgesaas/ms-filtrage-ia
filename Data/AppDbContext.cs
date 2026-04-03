using Microsoft.EntityFrameworkCore;
using ms_filtering_ia.Models;

namespace ms_filtering_ia.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<FilteringProfile> FilteringProfiles { get; set; }
    public DbSet<CriteriaType> CriteriaTypes { get; set; }
    public DbSet<FilteringResult> FilteringResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User → FilteringProfile (1,n)
        modelBuilder.Entity<FilteringProfile>()
            .HasOne(p => p.User)
            .WithMany(u => u.FilteringProfiles)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // FilteringProfile → CriteriaType (Many-to-Many)
        modelBuilder.Entity<FilteringProfile>()
            .HasMany(p => p.CriteriaTypes)
            .WithMany(t => t.FilteringProfiles)
            .UsingEntity(j => j.ToTable("FilteringProfile_CriteriaType"));

        // FilteringProfile → FilteringResult (1,n)
        modelBuilder.Entity<FilteringResult>()
            .HasOne(r => r.FilteringProfile)
            .WithMany(p => p.FilteringResults)
            .HasForeignKey(r => r.FilteringProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}