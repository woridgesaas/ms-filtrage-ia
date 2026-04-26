// Services/PdfExtractorService.cs
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System.Text;

namespace ms_filtering_ia.Services;

public class PdfExtractorService : IPdfExtractorService
{
    private readonly ILogger<PdfExtractorService> _logger;

    public PdfExtractorService(ILogger<PdfExtractorService> logger)
    {
        _logger = logger;
    }

    public string ExtractText(Stream pdfStream)
    {
        var sb = new StringBuilder();

        using var pdf = PdfDocument.Open(pdfStream);

        foreach (var page in pdf.GetPages())
        {
            // Récupère TOUS les mots avec leurs coordonnées Y (position verticale)
            var words = page.GetWords().ToList();

            if (!words.Any()) continue;

            // Trie par position Y décroissante (haut → bas) puis X croissante (gauche → droite)
            // Cela reconstitue l'ordre de lecture naturel même pour les PDFs multi-colonnes
            var sortedWords = words
                .OrderByDescending(w => w.BoundingBox.Bottom)
                .ThenBy(w => w.BoundingBox.Left)
                .ToList();

            // Regroupe les mots sur la même ligne (même Y ± 3 pixels de tolérance)
            var lines = new List<List<Word>>();
            List<Word>? currentLine = null;
            double? lastY = null;

            foreach (var word in sortedWords)
            {
                var y = Math.Round(word.BoundingBox.Bottom, 0);

                if (lastY == null || Math.Abs(y - lastY.Value) > 3)
                {
                    currentLine = new List<Word>();
                    lines.Add(currentLine);
                    lastY = y;
                }

                currentLine!.Add(word);
            }

            // Reconstruit le texte ligne par ligne
            foreach (var line in lines)
            {
                var lineText = string.Join(" ", line
                    .OrderBy(w => w.BoundingBox.Left)
                    .Select(w => w.Text));

                if (!string.IsNullOrWhiteSpace(lineText))
                    sb.AppendLine(lineText);
            }

            sb.AppendLine(); // séparateur entre pages
        }

        var text = sb.ToString().Trim();

        _logger.LogInformation(
            "Texte extrait depuis PDF : {CharCount} caractères sur {Pages} page(s)",
            text.Length, pdf.NumberOfPages
        );

        return text;
    }
}