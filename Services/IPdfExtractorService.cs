// Services/IPdfExtractorService.cs
namespace ms_filtering_ia.Services;

public interface IPdfExtractorService
{
    string ExtractText(Stream pdfStream);
}