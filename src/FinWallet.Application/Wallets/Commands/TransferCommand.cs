using AutoMapper;
using FinWallet.Application.Common.Exceptions;
using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Interfaces.Repositories;
using FinWallet.Application.Common.Models;
using FinWallet.Application.Transactions.Events;
using FinWallet.Domain.Enums;
using FluentValidation;
using MediatR;

namespace FinWallet.Application.Wallets.Commands;

public sealed record TransferCommand(Guid FromWalletId, Guid ToWalletId, decimal Amount, string Reference)
    : IRequest<IReadOnlyList<TransactionDto>>;

public sealed class TransferCommandValidator : AbstractValidator<TransferCommand>
{
    public TransferCommandValidator()
    {
        RuleFor(x => x.FromWalletId)
            .NotEmpty();

        RuleFor(x => x.ToWalletId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Reference)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x)
            .Must(x => x.FromWalletId != x.ToWalletId)
            .WithMessage("Vi nguon va vi dich khong duoc trung nhau.");
    }
}

public sealed class TransferCommandHandler : IRequestHandler<TransferCommand, IReadOnlyList<TransactionDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IWalletRepository _wallets;
    private readonly ITransactionRepository _transactions;
    private readonly IDistributedLock _distributedLock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly IEventBus _eventBus;
    private readonly IDateTimeProvider _dateTimeProvider;

    public TransferCommandHandler(
        ICurrentUserService currentUser,
        IWalletRepository wallets,
        ITransactionRepository transactions,
        IDistributedLock distributedLock,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper,
        IEventBus eventBus,
        IDateTimeProvider dateTimeProvider)
    {
        _currentUser = currentUser;
        _wallets = wallets;
        _transactions = transactions;
        _distributedLock = distributedLock;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
        _eventBus = eventBus;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<IReadOnlyList<TransactionDto>> Handle(TransferCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        if (userId is null)
        {
            throw new UnauthorizedException("Khong tim thay thong tin nguoi dung.");
        }

        var fromWallet = await _wallets.GetByIdAsync(request.FromWalletId, cancellationToken);
        var toWallet = await _wallets.GetByIdAsync(request.ToWalletId, cancellationToken);

        if (fromWallet is null || toWallet is null)
        {
            throw new NotFoundException("Khong tim thay vi.");
        }

        if (fromWallet.UserId != userId)
        {
            throw new ForbiddenException("Khong du quyen truy cap vi nguon.");
        }

        if (!string.Equals(fromWallet.Currency, toWallet.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException("Khong ho tro chuyen doi tien te.");
        }

        // Ghi chu: khoa theo thu tu guid de tranh deadlock.
        var firstLock = fromWallet.Id.CompareTo(toWallet.Id) < 0 ? fromWallet.Id : toWallet.Id;
        var secondLock = fromWallet.Id.CompareTo(toWallet.Id) < 0 ? toWallet.Id : fromWallet.Id;

        using var firstHandle = await _distributedLock.AcquireAsync(
            $"wallet:{firstLock}",
            TimeSpan.FromSeconds(10),
            cancellationToken);

        if (firstHandle is null)
        {
            throw new ConflictException("Vi dang duoc xu ly, vui long thu lai.");
        }

        using var secondHandle = await _distributedLock.AcquireAsync(
            $"wallet:{secondLock}",
            TimeSpan.FromSeconds(10),
            cancellationToken);

        if (secondHandle is null)
        {
            throw new ConflictException("Vi dang duoc xu ly, vui long thu lai.");
        }

        var debitTx = fromWallet.ApplyDebit(
            request.Amount,
            request.Reference,
            TransactionType.TransferOut,
            toWallet.Id);

        var creditTx = toWallet.ApplyCredit(
            request.Amount,
            request.Reference,
            TransactionType.TransferIn,
            fromWallet.Id);

        _transactions.Add(debitTx);
        _transactions.Add(creditTx);
        _wallets.Update(fromWallet);
        _wallets.Update(toWallet);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var transferId = Guid.NewGuid();
        await _eventBus.PublishAsync(
            new TransferRequestedIntegrationEvent(
                transferId,
                fromWallet.Id,
                toWallet.Id,
                request.Amount,
                request.Reference,
                _dateTimeProvider.UtcNow),
            cancellationToken);

        await _cache.RemoveAsync($"wallet:{fromWallet.Id}", cancellationToken);
        await _cache.RemoveAsync($"wallet:{toWallet.Id}", cancellationToken);

        return _mapper.Map<IReadOnlyList<TransactionDto>>(new[] { debitTx, creditTx });
    }
}
