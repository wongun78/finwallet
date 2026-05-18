using FinWallet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinWallet.Infrastructure.Persistence.Configurations;

public sealed class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.ToTable("wallet_transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasColumnType("numeric(18,2)");

        builder.Property(x => x.BalanceBefore)
            .HasColumnType("numeric(18,2)");

        builder.Property(x => x.BalanceAfter)
            .HasColumnType("numeric(18,2)");

        builder.Property(x => x.Reference)
            .HasMaxLength(200);

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.CounterpartyWalletId)
            .IsRequired(false);

        builder.HasIndex(x => x.WalletId);
    }
}
