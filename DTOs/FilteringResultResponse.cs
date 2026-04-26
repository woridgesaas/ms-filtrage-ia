namespace ms_filtering_ia.DTOs;

public sealed class FilteringResultResponse
{
    public int          Id               { get; set; }
    public string       CandidateName    { get; set; } = string.Empty;
    public string       CandidateEmail   { get; set; } = string.Empty;
    public string       CandidatePhone   { get; set; } = string.Empty;
    public string       Location         { get; set; } = string.Empty;
    public double       ScoreIa          { get; set; }
    public int          ExperienceYears  { get; set; }
    public List<string> TechnicalSkills  { get; set; } = new();  // ← tech + tools
    public List<string> SoftSkills       { get; set; } = new();  // ← soft séparé
    public List<string> MissingSkills    { get; set; } = new();
    public string       Recommendation   { get; set; } = string.Empty;
    public string       MainDomain       { get; set; } = string.Empty;
    public string       SeniorityLevel   { get; set; } = string.Empty;
    public string       CvStatus         { get; set; } = string.Empty;
    public string       CvDownloadUrl    { get; set; } = string.Empty;
    public DateTime     AnalysisDate     { get; set; }
}