namespace ms_filtering_ia.Services;

public interface IOcrService
{
    /// <summary>Extrait le texte d'un PDF scanné via OCR Tesseract.</summary>
    string ExtractTextFromImages(Stream pdfStream);
}