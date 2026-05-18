using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Models;

namespace FinWallet.Tests.Integration.Testing.Fakes;

public sealed class InMemoryObjectStorage : IObjectStorage
{
    public Task<ObjectStorageResult> PutObjectAsync(string objectKey, string contentType, byte[] data, CancellationToken ct)
    {
        var url = $"http://localhost/fake/{objectKey}";
        return Task.FromResult(new ObjectStorageResult(objectKey, url));
    }
}
