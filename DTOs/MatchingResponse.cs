namespace ms_filtering_ia.DTOs;

/// <summary>
/// Réponse de POST /api/v1/match.
/// Python retourne tous les CVs scorés en une seule réponse.
/// </summary>
public sealed class MatchingResponse
{
    public string              JobTitle      { get; set; } = string.Empty;
    public int                 TotalCvsInDb  { get; set; }
    public int                 CvsRetrieved  { get; set; }
    public int                 CvsScored     { get; set; }
    public List<CandidateMatch> Candidates   { get; set; } = new();
}

public sealed class CandidateMatch
{
    public string       CvId             { get; set; } = string.Empty;
    public string       Name             { get; set; } = string.Empty;
    public double       MatchScore       { get; set; }
    public string       Recommendation   { get; set; } = string.Empty;
    public List<string> MatchingSkills   { get; set; } = new();
    public List<string> MissingSkills    { get; set; } = new();
    public List<string> Strengths        { get; set; } = new();
    public List<string> Weaknesses       { get; set; } = new();
    public string       ExperienceMatch  { get; set; } = string.Empty;
    public string       EducationMatch   { get; set; } = string.Empty;
    public double       SimilarityScore  { get; set; }
}