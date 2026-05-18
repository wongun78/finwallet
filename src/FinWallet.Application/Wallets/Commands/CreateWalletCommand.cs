using AutoMapper;
using FinWallet.Application.Common.Exceptions;
using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Interfaces.Repositories;
using FinWallet.Application.Common.Models;
using FinWallet.Domain.Entities;
using FluentValidation;
using MediatR;

namespace FinWallet.Application.Wallets.Commands;

public sealed record CreateWalletCommand(string Currency) : IRequest<WalletDto>;

public sealed class CreateWalletCommandValidator : AbstractValidator<CreateWalletCommand>
{
    public CreateWalletCommandValidator()
    {
        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3);
    }
}

public sealed class CreateWalletCommandHandler : IRequestHandler<CreateWalletCommand, WalletDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IWalletRepository _wallets;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateWalletCommandHandler(
        ICurrentUserService currentUser,
        IWalletRepository wallets,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _currentUser = currentUser;
        _wallets = wallets;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<WalletDto> Handle(CreateWalletCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        if (userId is null)
        {
            throw new UnauthorizedException("Khong tim thay thong tin nguoi dung.");
        }

        var wallet = new Wallet(userId.Value, request.Currency);
        _wallets.Add(wallet);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WalletDto>(wallet);
    }
}
