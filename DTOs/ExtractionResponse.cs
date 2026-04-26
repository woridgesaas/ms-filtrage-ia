namespace ms_filtering_ia.DTOs;

/// <summary>
/// Réponse de POST /api/v1/extract.
/// Python retourne { cv_id, data: {...}, from_cache }
/// </summary>
public sealed class ExtractionResponse
{
    public string          CvId      { get; set; } = string.Empty;
    public ExtractionData  Data      { get; set; } = new();
    public bool            FromCache { get; set; }
}

public sealed class ExtractionData
{
    public PersonalInfoDto    PersonalInfo         { get; set; } = new();
    public List<EducationDto> Education            { get; set; } = new();
    public List<string>       TechnicalSkills      { get; set; } = new();
    public List<string>       Tools                { get; set; } = new();
    public List<string>       SoftSkills           { get; set; } = new();
    public List<LanguageDto>  Languages            { get; set; } = new();
    public List<string>       Certifications       { get; set; } = new();
    public int                TotalExperienceYears { get; set; }
    public string             SeniorityLevel       { get; set; } = string.Empty;
    public string             MainDomain           { get; set; } = string.Empty;
}

public sealed class PersonalInfoDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
    public string Phone     { get; set; } = string.Empty;
    public string Location  { get; set; } = string.Empty;
}

public sealed class EducationDto
{
    public string Degree { get; set; } = string.Empty;
}

public sealed class LanguageDto
{
    public string Language { get; set; } = string.Empty;
    public string Level    { get; set; } = string.Empty;
}