using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Models;
using FinWallet.Application.Common.Options;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace FinWallet.Infrastructure.Storage;

public sealed class MinioObjectStorage : IObjectStorage
{
    private readonly IMinioClient _minioClient;
    private readonly MinioOptions _options;

    public MinioObjectStorage(IMinioClient minioClient, IOptions<MinioOptions> options)
    {
        _minioClient = minioClient;
        _options = options.Value;
    }

    public async Task<ObjectStorageResult> PutObjectAsync(
        string objectKey,
        string contentType,
        byte[] data,
        CancellationToken ct)
    {
        await EnsureBucketAsync(ct);

        using var stream = new MemoryStream(data);

        var putArgs = new PutObjectArgs()
            .WithBucket(_options.Bucket)
            .WithObject(objectKey)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putArgs, ct);

        var publicEndpoint = _options.PublicEndpoint.TrimEnd('/');
        var url = string.IsNullOrWhiteSpace(publicEndpoint)
            ? $"{_options.Endpoint}/{_options.Bucket}/{objectKey}"
            : $"{publicEndpoint}/{_options.Bucket}/{objectKey}";

        return new ObjectStorageResult(objectKey, url);
    }

    private async Task EnsureBucketAsync(CancellationToken ct)
    {
        var existsArgs = new BucketExistsArgs().WithBucket(_options.Bucket);
        var exists = await _minioClient.BucketExistsAsync(existsArgs, ct);
        if (exists)
        {
            return;
        }

        var makeArgs = new MakeBucketArgs().WithBucket(_options.Bucket);
        await _minioClient.MakeBucketAsync(makeArgs, ct);
    }
}
