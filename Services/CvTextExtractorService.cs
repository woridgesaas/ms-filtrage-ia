// Services/CvTextExtractorService.cs
namespace ms_filtering_ia.Services;

public sealed class CvTextExtractorService : ICvTextExtractorService
{
    private readonly IMinioService _minio;
    private readonly IPdfExtractorService _pdfExtractor;
    private readonly IOcrService _ocr;
    private readonly ILogger<CvTextExtractorService> _logger;

    private const int MinPdfLength = 50;
    private const int MinOcrLength = 20;

    public CvTextExtractorService(
        IMinioService minio,
        IPdfExtractorService pdfExtractor,
        IOcrService ocr,
        ILogger<CvTextExtractorService> logger)
    {
        _minio = minio;
        _pdfExtractor = pdfExtractor;
        _ocr = ocr;
        _logger = logger;
    }

    public async Task<string> ExtractAsync(string cvMinioPath)
    {
        var pdfBytes = await _minio.GetCvBytesAsync(cvMinioPath);

        using var stream1 = new MemoryStream(pdfBytes);
        var text = _pdfExtractor.ExtractText(stream1);

        if (!string.IsNullOrWhiteSpace(text) && text.Length >= MinPdfLength)
            return text;

        _logger.LogWarning(
            "[CvTextExtractorService] PDF scanné ({Len} chars) → OCR : {Path}",
            text.Length, cvMinioPath);

        using var stream2 = new MemoryStream(pdfBytes);
        text = _ocr.ExtractTextFromImages(stream2);

        if (string.IsNullOrWhiteSpace(text) || text.Length < MinOcrLength)
            throw new InvalidOperationException(
                $"CV illisible même après OCR : {cvMinioPath}");

        return text;
    }
}