using System.Collections.Concurrent;
using FinWallet.Application.Common.Interfaces;

namespace FinWallet.Tests.Integration.Testing.Fakes;

public sealed class InMemoryDistributedLock : IDistributedLock
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

    public async Task<IDisposable?> AcquireAsync(string key, TimeSpan ttl, CancellationToken ct)
    {
        var semaphore = Locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        var acquired = await semaphore.WaitAsync(TimeSpan.FromMilliseconds(500), ct);

        return acquired ? new LockHandle(semaphore) : null;
    }

    private sealed class LockHandle : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        public LockHandle(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public void Dispose()
        {
            _semaphore.Release();
        }
    }
}
