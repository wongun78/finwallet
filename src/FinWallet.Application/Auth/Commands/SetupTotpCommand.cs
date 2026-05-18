using FinWallet.Application.Common.Exceptions;
using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Interfaces.Repositories;
using FinWallet.Application.Common.Models;
using MediatR;

namespace FinWallet.Application.Auth.Commands;

public sealed record SetupTotpCommand() : IRequest<TotpSetupDto>;

public sealed class SetupTotpCommandHandler : IRequestHandler<SetupTotpCommand, TotpSetupDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUserRepository _users;
    private readonly ITotpService _totpService;
    private readonly IUnitOfWork _unitOfWork;

    public SetupTotpCommandHandler(
        ICurrentUserService currentUser,
        IUserRepository users,
        ITotpService totpService,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _users = users;
        _totpService = totpService;
        _unitOfWork = unitOfWork;
    }

    public async Task<TotpSetupDto> Handle(SetupTotpCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        if (userId is null)
        {
            throw new UnauthorizedException("Khong tim thay thong tin nguoi dung.");
        }

        var user = await _users.GetByIdAsync(userId.Value, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("Khong tim thay nguoi dung.");
        }

        var secret = _totpService.GenerateSecret();
        user.SetupTotpSecret(secret);
        _users.Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var otpAuthUri = _totpService.BuildOtpAuthUri("FinWallet", user.Email, secret);
        return new TotpSetupDto(secret, otpAuthUri);
    }
}
