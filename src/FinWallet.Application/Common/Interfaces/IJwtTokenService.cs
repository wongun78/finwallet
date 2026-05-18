using FinWallet.Application.Common.Models;

namespace FinWallet.Application.Common.Interfaces;

public interface IJwtTokenService
{
    JwtTokenResult GenerateToken(Guid userId, string email);
}
