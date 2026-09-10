using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace Membera.Shared.Storage;

public class MinioFileStorageService : IFileStorageService
{
    // Local-dev MinIO endpoint. docker-compose.yml exposes the S3 API on 9000.
    private const string Endpoint = "localhost:9000";

    private readonly IMinioClient _minioClient;
    private readonly ILogger<MinioFileStorageService> _logger;

    public MinioFileStorageService(ILogger<MinioFileStorageService> logger)
    {
        _logger = logger;

        var accessKey = Environment.GetEnvironmentVariable("MINIO_ROOT_USER")
                        ?? throw new InvalidOperationException("MINIO_ROOT_USER environment variable is not set.");
        var secretKey = Environment.GetEnvironmentVariable("MINIO_ROOT_PASSWORD")
                        ?? throw new InvalidOperationException("MINIO_ROOT_PASSWORD environment variable is not set.");

        _minioClient = new MinioClient()
            .WithEndpoint(Endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(false) // non-SSL for local dev
            .Build();
    }

    public async Task<string> UploadFileAsync(string bucketName, string objectName, Stream fileStream, string contentType)
    {
        try
        {
            var bucketExists = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(bucketName));

            if (!bucketExists)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
                _logger.LogInformation("Created MinIO bucket: {BucketName}", bucketName);

                // Make the freshly-created bucket world-readable so the direct URLs
                // returned by GetFileUrl load in the browser without auth.
                //
                // NOTE: a public-read bucket is deliberately NOT how you'd serve
                // sensitive data in production - there you'd keep the bucket private
                // and hand out short-lived presigned URLs, or front it with a CDN
                // using signed URLs. It's fine (and much simpler) for this local dev
                // / portfolio project because the objects here are business logos and
                // subscription-plan photos, which aren't sensitive.
                //
                // Only done once, right after creation - if the bucket already
                // exists we assume the policy was set when it was created and skip
                // the extra API call.
                var publicReadPolicy = $$"""
                {
                  "Version": "2012-10-17",
                  "Statement": [
                    {
                      "Effect": "Allow",
                      "Principal": { "AWS": ["*"] },
                      "Action": ["s3:GetObject"],
                      "Resource": ["arn:aws:s3:::{{bucketName}}/*"]
                    }
                  ]
                }
                """;

                await _minioClient.SetPolicyAsync(new SetPolicyArgs()
                    .WithBucket(bucketName)
                    .WithPolicy(publicReadPolicy));

                _logger.LogInformation("Set public-read policy on MinIO bucket: {BucketName}", bucketName);
            }

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType));

            _logger.LogInformation("Uploaded file to MinIO: {BucketName}/{ObjectName}", bucketName, objectName);

            return objectName;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to upload file to MinIO: {BucketName}/{ObjectName}", bucketName, objectName);
            throw;
        }
    }

    public async Task DeleteFileAsync(string bucketName, string objectName)
    {
        try
        {
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName));

            _logger.LogInformation("Deleted file from MinIO: {BucketName}/{ObjectName}", bucketName, objectName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete file from MinIO: {BucketName}/{ObjectName}", bucketName, objectName);
            throw;
        }
    }

    // Builds a direct public URL. This dev setup configures the buckets for
    // public-read access; production would instead serve files through presigned
    // URLs or a CDN rather than exposing the bucket publicly.
    public string GetFileUrl(string bucketName, string objectName)
        => $"http://{Endpoint}/{bucketName}/{objectName}";
}
