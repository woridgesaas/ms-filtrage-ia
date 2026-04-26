namespace ms_filtering_ia.DTOs;

/// <summary>
/// Corps envoyé à POST /api/v1/match.
/// Python cherche dans ChromaDB tous les CVs indexés et les score.
/// </summary>
public sealed class MatchingRequest
{
    public JobOfferDto JobOffer            { get; set; } = new();
    public int         TopK                { get; set; } = 10;
    public double      SimilarityThreshold { get; set; } = 0.3;
    public int         MaxResults          { get; set; } = 20;
}

public sealed class JobOfferDto
{
    public string             JobTitle                { get; set; } = string.Empty;
    public List<string>       RequiredSkills          { get; set; } = new();
    public List<string>       RequiredTools           { get; set; } = new();
    public int                RequiredExperienceYears { get; set; }
    public string             RequiredDegree          { get; set; } = string.Empty;
    public List<LangRequirement> RequiredLanguages    { get; set; } = new();
    public string             Location                { get; set; } = string.Empty;
    public List<string>       Certifications          { get; set; } = new();
    public List<string>       SoftSkills              { get; set; } = new();
}

public sealed class LangRequirement
{
    public string Lang  { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
}