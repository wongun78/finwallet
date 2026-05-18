namespace FinWallet.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? GetUserId();
    string? GetEmail();
}
