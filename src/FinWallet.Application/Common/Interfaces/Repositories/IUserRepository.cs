using FinWallet.Domain.Entities;

namespace FinWallet.Application.Common.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    void Add(User user);
    void Update(User user);
}
