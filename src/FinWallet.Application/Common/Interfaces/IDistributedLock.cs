namespace FinWallet.Application.Common.Interfaces;

public interface IDistributedLock
{
    Task<IDisposable?> AcquireAsync(string key, TimeSpan ttl, CancellationToken ct);
}
