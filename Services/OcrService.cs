using System.Text;
using Microsoft.Extensions.Options;
using ms_filtering_ia.Options;
using Tesseract;
using UglyToad.PdfPig;

namespace ms_filtering_ia.Services;

public sealed class OcrService : IOcrService
{
    private readonly TesseractOptions    _options;
    private readonly ILogger<OcrService> _logger;

    public OcrService(IOptions<TesseractOptions> options, ILogger<OcrService> logger)
    {
        _options = options.Value;
        _logger  = logger;
    }

    public string ExtractTextFromImages(Stream pdfStream)
    {
        var sb = new StringBuilder();

        using var engine = new TesseractEngine(_options.DataPath, _options.Languages, EngineMode.Default);
        using var pdf    = PdfDocument.Open(pdfStream);

        foreach (var page in pdf.GetPages())
        {
            var images = page.GetImages().ToList();

            if (images.Count == 0)
            {
                _logger.LogWarning("[OcrService] Page {Num} sans image — ignorée.", page.Number);
                continue;
            }

            foreach (var pdfImage in images)
            {
                try
                {
                    if (!pdfImage.TryGetPng(out var pngBytes))
                    {
                        _logger.LogWarning("[OcrService] Image non convertible en PNG — ignorée.");
                        continue;
                    }

                    using var pix     = Pix.LoadFromMemory(pngBytes.ToArray());
                    using var ocrPage = engine.Process(pix);
                    var text = ocrPage.GetText();

                    if (!string.IsNullOrWhiteSpace(text))
                        sb.AppendLine(text);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[OcrService] Erreur traitement image — ignorée.");
                }
            }
        }

        var result = sb.ToString().Trim();
        _logger.LogInformation("[OcrService] OCR terminé : {Length} chars.", result.Length);
        return result;
    }
}