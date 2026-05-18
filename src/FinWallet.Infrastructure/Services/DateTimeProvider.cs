using FinWallet.Application.Common.Interfaces;

namespace FinWallet.Infrastructure.Services;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
