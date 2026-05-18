using AutoMapper;
using FinWallet.Application.Common.Exceptions;
using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Interfaces.Repositories;
using FinWallet.Application.Common.Models;
using MediatR;

namespace FinWallet.Application.Wallets.Queries;

public sealed record GetUserWalletsQuery() : IRequest<IReadOnlyList<WalletDto>>;

public sealed class GetUserWalletsQueryHandler : IRequestHandler<GetUserWalletsQuery, IReadOnlyList<WalletDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IWalletRepository _wallets;
    private readonly IMapper _mapper;

    public GetUserWalletsQueryHandler(
        ICurrentUserService currentUser,
        IWalletRepository wallets,
        IMapper mapper)
    {
        _currentUser = currentUser;
        _wallets = wallets;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<WalletDto>> Handle(GetUserWalletsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        if (userId is null)
        {
            throw new UnauthorizedException("Khong tim thay thong tin nguoi dung.");
        }

        var items = await _wallets.GetByUserIdAsync(userId.Value, cancellationToken);
        return _mapper.Map<IReadOnlyList<WalletDto>>(items);
    }
}
