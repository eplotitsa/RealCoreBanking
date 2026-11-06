using RealCoreBanking.Core.Contracts;
using RealCoreBanking.Core.Models;
using Xunit;

namespace RealCoreBanking.Tests;

public class WithdrawSmartContractTests
{
    private const long MainAddr  = 100_000_001;   // prefix 1 — main balance
    private const long OdUsedAddr = 500_000_001;  // prefix 5 — used overdraft
    private const long OdLimAddr  = 600_000_001;  // prefix 6 — overdraft limit

    private static Balance Bal(long addr, int prefix, decimal amount, string cur = "USD") => new()
    {
        Address = addr, BalanceTypePrefix = prefix, Amount = amount, Currency = cur,
    };

    private static CurrentAccount Account(decimal main, decimal odLimit = 0, decimal odUsed = 0)
    {
        var acc = new CurrentAccount();
        acc.Balances.Add(Bal(MainAddr, 1, main));
        if (odLimit > 0 || odUsed > 0)
        {
            acc.Balances.Add(Bal(OdUsedAddr, 5, odUsed));
            acc.Balances.Add(Bal(OdLimAddr, 6, odLimit));
        }
        return acc;
    }

    [Fact]
    public async Task Withdraw_WithSufficientFunds_DebitsMainBalance()
    {
        var acc = Account(100m);
        var sc = new WithdrawSmartContract(40m, MainAddr);

        await sc.ExecuteAsync(acc);

        Assert.True(sc.Success);
        Assert.Equal(60m, acc.Balances.GetByAddress(MainAddr)!.Amount);
        var tx = Assert.Single(acc.Transactions);
        Assert.Equal("Posted", tx.Status);
        Assert.Equal(40m, tx.Amount);
        Assert.Single(tx.Postings);
    }

    [Fact]
    public async Task Withdraw_ExactBalance_LeavesZero()
    {
        var acc = Account(100m);
        var sc = new WithdrawSmartContract(100m, MainAddr);

        await sc.ExecuteAsync(acc);

        Assert.True(sc.Success);
        Assert.Equal(0m, acc.Balances.GetByAddress(MainAddr)!.Amount);
    }

    [Fact]
    public async Task Withdraw_ExceedingOtb_IsRejectedAndNothingChanges()
    {
        var acc = Account(100m);
        var sc = new WithdrawSmartContract(100.01m, MainAddr);

        await sc.ExecuteAsync(acc);

        Assert.False(sc.Success);
        Assert.Equal(100m, acc.Balances.GetByAddress(MainAddr)!.Amount);
        Assert.Empty(acc.Transactions);
    }

    [Fact]
    public async Task Withdraw_UnknownAddress_Fails()
    {
        var acc = Account(100m);
        var sc = new WithdrawSmartContract(10m, 999);

        await sc.ExecuteAsync(acc);

        Assert.False(sc.Success);
        Assert.Empty(acc.Transactions);
    }

    [Fact]
    public async Task Withdraw_IntoOverdraft_ZeroesMainAndRecordsOverdraftUsage()
    {
        var acc = Account(main: 100m, odLimit: 50m);
        var sc = new WithdrawSmartContract(120m, MainAddr);

        await sc.ExecuteAsync(acc);

        Assert.True(sc.Success);
        Assert.Equal(0m,  acc.Balances.GetByAddress(MainAddr)!.Amount);
        Assert.Equal(20m, acc.Balances.GetByAddress(OdUsedAddr)!.Amount);
        Assert.Equal(50m, acc.Balances.GetByAddress(OdLimAddr)!.Amount); // limit is never reduced

        var tx = Assert.Single(acc.Transactions);
        Assert.Equal(2, tx.Postings.Count);
        Assert.Equal(20m, tx.Postings[1].Amount);
        Assert.Equal(OdUsedAddr, tx.Postings[1].CrAddress);
    }

    [Fact]
    public async Task Withdraw_BeyondOverdraftLimit_IsRejected()
    {
        var acc = Account(main: 100m, odLimit: 50m);
        var sc = new WithdrawSmartContract(151m, MainAddr);

        await sc.ExecuteAsync(acc);

        Assert.False(sc.Success);
        Assert.Equal(100m, acc.Balances.GetByAddress(MainAddr)!.Amount);
        Assert.Equal(0m,   acc.Balances.GetByAddress(OdUsedAddr)!.Amount);
    }

    [Fact]
    public async Task Withdraw_AlreadyUsedOverdraft_ReducesAvailableOtb()
    {
        // OTB = 0 + 50 - 30 = 20, so 25 must be rejected and 20 accepted.
        var acc = Account(main: 0m, odLimit: 50m, odUsed: 30m);

        var tooMuch = new WithdrawSmartContract(25m, MainAddr);
        await tooMuch.ExecuteAsync(acc);
        Assert.False(tooMuch.Success);

        var ok = new WithdrawSmartContract(20m, MainAddr);
        await ok.ExecuteAsync(acc);
        Assert.True(ok.Success);
        Assert.Equal(50m, acc.Balances.GetByAddress(OdUsedAddr)!.Amount);
    }

    [Fact]
    public async Task Withdraw_OtbIgnoresBalancesInOtherCurrencies()
    {
        var acc = Account(100m);
        acc.Balances.Add(Bal(100_000_002, 1, 1_000m, "EUR"));
        var sc = new WithdrawSmartContract(150m, MainAddr); // USD only has 100

        await sc.ExecuteAsync(acc);

        Assert.False(sc.Success);
    }

    [Fact]
    public async Task Withdraw_BlockedFundsAreNotAvailable()
    {
        var acc = Account(100m);
        acc.Balances.Add(Bal(400_000_001, 4, 70m)); // block
        var sc = new WithdrawSmartContract(40m, MainAddr); // OTB = 30

        await sc.ExecuteAsync(acc);

        Assert.False(sc.Success);
    }

    [Fact]
    public async Task Withdraw_IntoOverdraftWithoutUsedBalance_MustNotLoseDeficit()
    {
        // Limit exists but no prefix-5 balance: the 20 deficit must not silently vanish.
        var acc = new CurrentAccount();
        acc.Balances.Add(Bal(MainAddr, 1, 100m));
        acc.Balances.Add(Bal(OdLimAddr, 6, 50m));

        var sc = new WithdrawSmartContract(120m, MainAddr);
        await sc.ExecuteAsync(acc);

        var main = acc.Balances.GetByAddress(MainAddr)!.Amount;
        Assert.True(!sc.Success || main < 0 || acc.Balances.Items.Any(b => b.Prefix == 5 && b.Amount == 20m),
            "Deficit of 20 was lost: main balance zeroed but overdraft usage not recorded.");
    }
}
