// Services/ICvTextExtractorService.cs
namespace ms_filtering_ia.Services;

public interface ICvTextExtractorService
{
    /// <summary>Télécharge et extrait le texte d'un CV depuis MinIO (PDF + OCR fallback).</summary>
    Task<string> ExtractAsync(string cvMinioPath);
}