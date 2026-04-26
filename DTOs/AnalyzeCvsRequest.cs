namespace ms_filtering_ia.DTOs;

/// <summary>
/// Reçu depuis Angular pour lancer le pipeline "Filtrer par IA".
/// Contient les critères de l'offre + l'identifiant du profil de filtrage.
/// </summary>
public sealed class AnalyzeCvsRequest
{
    public string OfferTitle { get; set; } = string.Empty;

    public List<string> TechSkills { get; set; } = new();
    public List<string> Tools { get; set; } = new();
    public List<string> SoftSkills { get; set; } = new();
    public List<string> Certifications { get; set; } = new();

    public string Degree { get; set; } = string.Empty;
    public int MinExperience { get; set; }

    public string Language { get; set; } = string.Empty;
    public string LanguageLevel { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    // Optionnel — null si aucun profil sélectionné par le RH
    public int FilteringProfileId { get; set; }
}