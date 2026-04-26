namespace ms_filtering_ia.Models;

public class FilteringResult
{
    public int    Id                  { get; set; }
    public string CvId                { get; set; } = string.Empty;
    public string CandidateName       { get; set; } = string.Empty;
    public string CandidateEmail      { get; set; } = string.Empty;
    public string CandidatePhone      { get; set; } = string.Empty;
    public string CandidateLocation   { get; set; } = string.Empty;
    public string CvMinioPath         { get; set; } = string.Empty;
    public double GlobalScore         { get; set; }
    public string MainDomain          { get; set; } = string.Empty;
    public string SeniorityLevel      { get; set; } = string.Empty;
    public string ExtractedSkills     { get; set; } = string.Empty;      // tech + tools CSV
    public string ExtractedSoftSkills { get; set; } = string.Empty;      // ← nouveau soft CSV
    public string MissingSkills       { get; set; } = string.Empty;
    public string Recommendation      { get; set; } = string.Empty;
    public int    ExperienceYears     { get; set; }
    public string Decision            { get; set; } = string.Empty;
    public string CvStatus            { get; set; } = "pending";
    public DateTime AnalysisDate      { get; set; } = DateTime.UtcNow;
    public int?   FilteringProfileId  { get; set; }
    public FilteringProfile? FilteringProfile { get; set; }
}