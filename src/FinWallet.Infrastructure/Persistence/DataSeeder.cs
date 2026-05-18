using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Options;
using FinWallet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FinWallet.Infrastructure.Persistence;

public sealed class DataSeeder
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly SeedOptions _options;

    public DataSeeder(
        ApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IOptions<SeedOptions> options)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _options = options.Value;
    }

    public async Task SeedAsync()
    {
        if (!_options.Enabled)
        {
            return;
        }

        var existing = await _dbContext.Users.AnyAsync(u => u.Email == _options.AdminEmail);
        if (existing)
        {
            return;
        }

        var admin = new User(_options.AdminEmail, _passwordHasher.Hash(_options.AdminPassword));
        _dbContext.Users.Add(admin);

        if (_options.SeedWallet)
        {
            var wallet = new Wallet(admin.Id, "VND");
            _dbContext.Wallets.Add(wallet);
        }

        await _dbContext.SaveChangesAsync();
    }
}
