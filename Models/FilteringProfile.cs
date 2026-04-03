namespace ms_filtering_ia.Models;

public class FilteringProfile
{
    public int Id { get; set; }
    public string ProfileName { get; set; } = string.Empty;
    public int OfferId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // FK vers User
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    // Navigation
    public ICollection<CriteriaType> CriteriaTypes { get; set; } = new List<CriteriaType>();
    public ICollection<FilteringResult> FilteringResults { get; set; } = new List<FilteringResult>();
}