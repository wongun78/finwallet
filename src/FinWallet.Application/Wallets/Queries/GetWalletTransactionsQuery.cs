using AutoMapper;
using FinWallet.Application.Common.Exceptions;
using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Interfaces.Repositories;
using FinWallet.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace FinWallet.Application.Wallets.Queries;

public sealed record GetWalletTransactionsQuery(Guid WalletId, int Page = 1, int PageSize = 20)
    : IRequest<IReadOnlyList<TransactionDto>>;

public sealed class GetWalletTransactionsQueryValidator : AbstractValidator<GetWalletTransactionsQuery>
{
    public GetWalletTransactionsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}

public sealed class GetWalletTransactionsQueryHandler
    : IRequestHandler<GetWalletTransactionsQuery, IReadOnlyList<TransactionDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IWalletRepository _wallets;
    private readonly ITransactionRepository _transactions;
    private readonly IMapper _mapper;

    public GetWalletTransactionsQueryHandler(
        ICurrentUserService currentUser,
        IWalletRepository wallets,
        ITransactionRepository transactions,
        IMapper mapper)
    {
        _currentUser = currentUser;
        _wallets = wallets;
        _transactions = transactions;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<TransactionDto>> Handle(
        GetWalletTransactionsQuery request,
        CancellationToken cancellationToken)
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

        var items = await _transactions.GetByWalletIdAsync(
            request.WalletId,
            request.Page,
            request.PageSize,
            cancellationToken);

        return _mapper.Map<IReadOnlyList<TransactionDto>>(items);
    }
}
