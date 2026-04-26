namespace ms_filtering_ia.DTOs;

public sealed class IndexRequest
{
    public string        CvText   { get; set; } = string.Empty;
    public IndexMetadata Metadata { get; set; } = new();
}

public sealed class IndexMetadata
{
    public string Filename { get; set; } = string.Empty;
    public string Source   { get; set; } = "minio";
}