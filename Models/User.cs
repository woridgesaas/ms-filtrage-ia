namespace ms_filtering_ia.Models;

public class User
{
    public int Id { get; set; }
    public string Cin { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // Navigation
    public ICollection<FilteringProfile> FilteringProfiles { get; set; } = new List<FilteringProfile>();
}