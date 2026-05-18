using AutoMapper;
using FinWallet.Application.Common.Exceptions;
using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Interfaces.Repositories;
using FinWallet.Application.Common.Models;
using MediatR;

namespace FinWallet.Application.Wallets.Queries;

public sealed record GetWalletByIdQuery(Guid WalletId) : IRequest<WalletDto>;

public sealed class GetWalletByIdQueryHandler : IRequestHandler<GetWalletByIdQuery, WalletDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IWalletRepository _wallets;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;

    public GetWalletByIdQueryHandler(
        ICurrentUserService currentUser,
        IWalletRepository wallets,
        ICacheService cache,
        IMapper mapper)
    {
        _currentUser = currentUser;
        _wallets = wallets;
        _cache = cache;
        _mapper = mapper;
    }

    public async Task<WalletDto> Handle(GetWalletByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        if (userId is null)
        {
            throw new UnauthorizedException("Khong tim thay thong tin nguoi dung.");
        }

        var cacheKey = $"wallet:{request.WalletId}";
        var cached = await _cache.GetAsync<WalletDto>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
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

        var dto = _mapper.Map<WalletDto>(wallet);
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(2), cancellationToken);

        return dto;
    }
}
