namespace ms_filtering_ia.Options;

public sealed class TesseractOptions
{
    public string DataPath  { get; set; } = "./tessdata";
    public string Languages { get; set; } = "fra+eng";
}