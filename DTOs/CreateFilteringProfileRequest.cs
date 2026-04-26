namespace ms_filtering_ia.DTOs;

public class CreateFilteringProfileRequest
{
    public string ProfileName { get; set; } = string.Empty;
    public int OfferId { get; set; }
    public List<string> CriteriaNames { get; set; } = new();
}