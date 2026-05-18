using FinWallet.Application.Common.Models;

namespace FinWallet.Application.Common.Interfaces;

public interface IObjectStorage
{
    Task<ObjectStorageResult> PutObjectAsync(
        string objectKey,
        string contentType,
        byte[] data,
        CancellationToken ct);
}
