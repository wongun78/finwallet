using AutoMapper;
using FinWallet.Application.Common.Exceptions;
using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Interfaces.Repositories;
using FinWallet.Application.Common.Models;
using FinWallet.Domain.Enums;
using FluentValidation;
using MediatR;

namespace FinWallet.Application.Wallets.Commands;

public sealed record PayWalletCommand(Guid WalletId, decimal Amount, string Reference) : IRequest<TransactionDto>;

public sealed class PayWalletCommandValidator : AbstractValidator<PayWalletCommand>
{
    public PayWalletCommandValidator()
    {
        RuleFor(x => x.WalletId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Reference)
            .NotEmpty()
            .MaximumLength(200);
    }
}

public sealed class PayWalletCommandHandler : IRequestHandler<PayWalletCommand, TransactionDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IWalletRepository _wallets;
    private readonly ITransactionRepository _transactions;
    private readonly IDistributedLock _distributedLock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;

    public PayWalletCommandHandler(
        ICurrentUserService currentUser,
        IWalletRepository wallets,
        ITransactionRepository transactions,
        IDistributedLock distributedLock,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper)
    {
        _currentUser = currentUser;
        _wallets = wallets;
        _transactions = transactions;
        _distributedLock = distributedLock;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
    }

    public async Task<TransactionDto> Handle(PayWalletCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        if (userId is null)
        {
            throw new UnauthorizedException("Khong tim thay thong tin nguoi dung.");
        }

        var wallet = await _wallets.GetByIdAsync(request.WalletId, cancellationToken);
        if (wallet is null)
        {
            throw new NotFoundException("Khong tim thay vi.");
        }

        if (wallet.UserId != userId)
        {
            throw new ForbiddenException("Khong du quyen truy cap vi.");
        }

        var lockKey = $"wallet:{wallet.Id}";
        using var handle = await _distributedLock.AcquireAsync(lockKey, TimeSpan.FromSeconds(10), cancellationToken);
        if (handle is null)
        {
            throw new ConflictException("Vi dang duoc xu ly, vui long thu lai.");
        }

        var tx = wallet.ApplyDebit(request.Amount, request.Reference, TransactionType.Payment);
        _transactions.Add(tx);
        _wallets.Update(wallet);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync($"wallet:{wallet.Id}", cancellationToken);

        return _mapper.Map<TransactionDto>(tx);
    }
}
