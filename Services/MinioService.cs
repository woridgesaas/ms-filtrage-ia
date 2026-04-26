using Minio;
using Minio.DataModel.Args;

namespace ms_filtering_ia.Services;

public sealed class MinioService : IMinioService
{
    private readonly IMinioClient        _minioClient;
    private readonly ILogger<MinioService> _logger;

    public MinioService(IMinioClient minioClient, ILogger<MinioService> logger)
    {
        _minioClient = minioClient;
        _logger      = logger;
    }

    public async Task<Stream> GetCvStreamAsync(string objectName, string bucketName = "cvs")
    {
        var ms = new MemoryStream();

        var args = new GetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithCallbackStream(s => s.CopyTo(ms));

        await _minioClient.GetObjectAsync(args);
        ms.Position = 0;

        _logger.LogInformation("[MinioService] CV récupéré : {Object}", objectName);
        return ms;
    }

    public async Task<byte[]> GetCvBytesAsync(string objectName, string bucketName = "cvs")
    {
        using var stream = await GetCvStreamAsync(objectName, bucketName);
        return ((MemoryStream)stream).ToArray();
    }

    public async Task<List<string>> ListCvsAsync(string bucketName = "cvs")
    {
        var keys = new List<string>();

        var args = new ListObjectsArgs()
            .WithBucket(bucketName)
            .WithRecursive(true);

        await foreach (var item in _minioClient.ListObjectsEnumAsync(args))
        {
            if (item.Key.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                keys.Add(item.Key);
                _logger.LogInformation("[MinioService] CV trouvé : {Key}", item.Key);
            }
        }

        _logger.LogInformation("[MinioService] Total : {Count} CVs.", keys.Count);
        return keys;
    }

    public async Task<string> GetPresignedUrlAsync(string objectName, int expirySeconds = 3600)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket("cvs")
            .WithObject(objectName)
            .WithExpiry(expirySeconds);

        return await _minioClient.PresignedGetObjectAsync(args);
    }
}