using FinWallet.Application.Common.Exceptions;
using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace FinWallet.Application.Auth.Commands;

public sealed record ConfirmTotpCommand(string Code) : IRequest<bool>;

public sealed class ConfirmTotpCommandValidator : AbstractValidator<ConfirmTotpCommand>
{
    public ConfirmTotpCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6, 8);
    }
}

public sealed class ConfirmTotpCommandHandler : IRequestHandler<ConfirmTotpCommand, bool>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUserRepository _users;
    private readonly ITotpService _totpService;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmTotpCommandHandler(
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

    public async Task<bool> Handle(ConfirmTotpCommand request, CancellationToken cancellationToken)
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

        if (string.IsNullOrWhiteSpace(user.TotpSecret) || !_totpService.Validate(user.TotpSecret, request.Code))
        {
            throw new UnauthorizedException("Ma TOTP khong hop le.");
        }

        user.EnableTotp();
        _users.Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
