using AutoMapper;
using FinWallet.Application.Common.Models;
using FinWallet.Domain.Entities;

namespace FinWallet.Application.Common.Mappings;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Ghi chu: map DTO de tra ve API.
        CreateMap<Wallet, WalletDto>()
            .ForMember(d => d.WalletId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<WalletTransaction, TransactionDto>()
            .ForMember(d => d.TransactionId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
    }
}
