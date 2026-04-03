namespace ms_filtering_ia.Models;

public class CriteriaType
{
    public int Id { get; set; }
    public string CriteriaName { get; set; } = string.Empty;

    // Navigation Many-to-Many
    public ICollection<FilteringProfile> FilteringProfiles { get; set; } = new List<FilteringProfile>();
}