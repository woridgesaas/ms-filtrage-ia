namespace ms_filtering_ia.Services;

public interface IMinioService
{
    Task<Stream>       GetCvStreamAsync(string objectName, string bucketName = "cvs");
    Task<byte[]>       GetCvBytesAsync(string objectName, string bucketName = "cvs");
    Task<List<string>> ListCvsAsync(string bucketName = "cvs");
    Task<string>       GetPresignedUrlAsync(string objectName, int expirySeconds = 3600);
}