namespace ms_filtering_ia.Models;

public class FilteringResult
{
    public int Id { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string CandidatePhone { get; set; } = string.Empty;
    public double GlobalScore { get; set; }
    public string Decision { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

    // FK vers FilteringProfile
    public int FilteringProfileId { get; set; }
    public FilteringProfile FilteringProfile { get; set; } = null!;
}