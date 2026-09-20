using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Data.Entities;

namespace RealCoreBanking.Data.Repositories;

public class EfInternalAccountRepository : IInternalAccountRepository
{
    private readonly IDbContextFactory<BankingDbContext> _factory;
    private readonly ITransactionRepository _txRepo;
    private static readonly JsonSerializerOptions _json = new() { WriteIndented = false };

    public EfInternalAccountRepository(
        IDbContextFactory<BankingDbContext> factory,
        ITransactionRepository txRepo)
    {
        _factory = factory;
        _txRepo  = txRepo;
    }

    public IReadOnlyList<InternalAccount> GetAll()
    {
        using var ctx = _factory.CreateDbContext();
        var accounts = ctx.InternalAccounts.AsNoTracking()
            .Select(e => new { e.AccountId, e.Data }).ToList();

        return accounts.Select(e =>
        {
            var a = JsonSerializer.Deserialize<InternalAccount>(e.Data, _json)!;
            a.Transactions = _txRepo.GetByAccount(e.AccountId).ToList();
            return a;
        }).Where(a => a != null).ToList();
    }

    public InternalAccount? Get(string accountId)
    {
        using var ctx = _factory.CreateDbContext();
        var entity = ctx.InternalAccounts.AsNoTracking()
            .FirstOrDefault(e => e.AccountId == accountId);
        if (entity == null) return null;

        var account = JsonSerializer.Deserialize<InternalAccount>(entity.Data, _json)!;
        account.Transactions = _txRepo.GetByAccount(accountId).ToList();
        return account;
    }

    public void Save(InternalAccount account)
    {
        using var ctx = _factory.CreateDbContext();

        var savedTxs = account.Transactions;
        account.Transactions = new List<AccountTransaction>();
        var json = JsonSerializer.Serialize(account, _json);
        account.Transactions = savedTxs;

        var entity = ctx.InternalAccounts.Find(account.AccountId);
        if (entity == null)
            ctx.InternalAccounts.Add(new InternalAccountEntity { AccountId = account.AccountId, Data = json });
        else
            entity.Data = json;
        ctx.SaveChanges();

        _txRepo.SaveAll(account.AccountId, account.Transactions);
    }

    public void Delete(string accountId)
    {
        using var ctx = _factory.CreateDbContext();
        _txRepo.DeleteByAccount(accountId);
        var entity = ctx.InternalAccounts.Find(accountId);
        if (entity != null) { ctx.InternalAccounts.Remove(entity); ctx.SaveChanges(); }
    }
}
