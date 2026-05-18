using FinWallet.Application.Common.Interfaces.Repositories;
using FinWallet.Domain.Entities;
using FinWallet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinWallet.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        return _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct);
    }

    public void Add(User user)
    {
        _dbContext.Users.Add(user);
    }

    public void Update(User user)
    {
        _dbContext.Users.Update(user);
    }
}
