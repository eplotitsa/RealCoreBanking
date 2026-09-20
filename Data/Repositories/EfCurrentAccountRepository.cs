using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Data.Entities;

namespace RealCoreBanking.Data.Repositories;

public class EfCurrentAccountRepository : ICurrentAccountRepository
{
    private readonly IDbContextFactory<BankingDbContext> _factory;
    private readonly ITransactionRepository _txRepo;

    // Serialise account WITHOUT the Transactions list — those live in their own table.
    private static readonly JsonSerializerOptions _json = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    };

    public EfCurrentAccountRepository(
        IDbContextFactory<BankingDbContext> factory,
        ITransactionRepository txRepo)
    {
        _factory = factory;
        _txRepo  = txRepo;
    }

    public IReadOnlyList<CurrentAccount> GetAll()
    {
        using var ctx = _factory.CreateDbContext();
        var accounts = ctx.CurrentAccounts.AsNoTracking()
            .Select(e => new { e.AccountId, e.Data }).ToList();

        return accounts.Select(e =>
        {
            var a = JsonSerializer.Deserialize<CurrentAccount>(e.Data, _json)!;
            a.Transactions = _txRepo.GetByAccount(e.AccountId).ToList();
            return a;
        }).Where(a => a != null).ToList();
    }

    public CurrentAccount? Get(string accountId)
    {
        using var ctx = _factory.CreateDbContext();
        var entity = ctx.CurrentAccounts.AsNoTracking()
            .FirstOrDefault(e => e.AccountId == accountId);
        if (entity == null) return null;

        var account = JsonSerializer.Deserialize<CurrentAccount>(entity.Data, _json)!;
        account.Transactions = _txRepo.GetByAccount(accountId).ToList();
        return account;
    }

    public void Save(CurrentAccount account)
    {
        using var ctx = _factory.CreateDbContext();

        // Persist account blob without transactions (cleared temporarily for serialisation)
        var savedTxs = account.Transactions;
        account.Transactions = new List<AccountTransaction>();
        var json = JsonSerializer.Serialize(account, _json);
        account.Transactions = savedTxs;

        var entity = ctx.CurrentAccounts.Find(account.AccountId);
        if (entity == null)
            ctx.CurrentAccounts.Add(new CurrentAccountEntity { AccountId = account.AccountId, Data = json });
        else
            entity.Data = json;
        ctx.SaveChanges();

        // Persist transactions in their own table
        _txRepo.SaveAll(account.AccountId, account.Transactions);
    }

    public void Delete(string accountId)
    {
        using var ctx = _factory.CreateDbContext();
        _txRepo.DeleteByAccount(accountId);   // remove transactions first (no cascade from account)
        var entity = ctx.CurrentAccounts.Find(accountId);
        if (entity != null) { ctx.CurrentAccounts.Remove(entity); ctx.SaveChanges(); }
    }
}
