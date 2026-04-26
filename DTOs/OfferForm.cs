namespace ms_filtering_ia.DTOs;

public sealed class OfferForm
{
    public string OfferTitle  { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public List<string> TechSkills     { get; set; } = new();
    public List<string> Tools          { get; set; } = new();
    public List<string> SoftSkills     { get; set; } = new();
    public List<string> Certifications { get; set; } = new();

    public string Degree        { get; set; } = string.Empty;
    public int    MinExperience { get; set; }

    public string Language      { get; set; } = string.Empty;
    public string LanguageLevel { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    // Usage interne uniquement
    public string CvMinioPath        { get; set; } = string.Empty;
    public int    FilteringProfileId { get; set; }
}