namespace ms_filtering_ia.DTOs;

public sealed class IndexResponse
{
    public string CvId   { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;  // "indexed" | "already_exists"
}