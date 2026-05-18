using System;
using FinWallet.Domain.Entities;
using FinWallet.Domain.Enums;
using FinWallet.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FinWallet.Tests.Unit.Domain;

public sealed class WalletTests
{
    [Fact]
    public void ApplyCredit_IncreasesBalance()
    {
        var wallet = new Wallet(Guid.NewGuid(), "VND");

        var tx = wallet.ApplyCredit(100_000m, "TOPUP", TransactionType.TopUp);

        wallet.Balance.Should().Be(100_000m);
        tx.BalanceAfter.Should().Be(100_000m);
    }

    [Fact]
    public void ApplyDebit_ThrowsWhenInsufficient()
    {
        var wallet = new Wallet(Guid.NewGuid(), "VND");

        var action = () => wallet.ApplyDebit(10_000m, "PAY", TransactionType.Payment);

        action.Should().Throw<DomainException>();
    }
}
