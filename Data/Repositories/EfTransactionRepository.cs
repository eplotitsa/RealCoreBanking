using Microsoft.EntityFrameworkCore;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Data.Entities;

namespace RealCoreBanking.Data.Repositories;

/// <summary>
/// EF Core repository for account transactions and their double-entry postings.
/// Safe for singleton injection via IDbContextFactory.
/// </summary>
public sealed class EfTransactionRepository : ITransactionRepository
{
    private readonly IDbContextFactory<BankingDbContext> _factory;

    public EfTransactionRepository(IDbContextFactory<BankingDbContext> factory)
        => _factory = factory;

    // ── Read ──────────────────────────────────────────────────────────────────

    public IReadOnlyList<AccountTransaction> GetByAccount(string accountId)
    {
        using var ctx = _factory.CreateDbContext();
        return ctx.Transactions
            .Where(t => t.AccountId == accountId)
            .Include(t => t.Postings)
            .AsNoTracking()
            .OrderBy(t => t.Date)
            .ToList()
            .Select(ToDomain)
            .ToList();
    }

    // ── Write ─────────────────────────────────────────────────────────────────

    public void Save(string accountId, AccountTransaction tx)
    {
        using var ctx = _factory.CreateDbContext();
        UpsertTx(ctx, accountId, tx);
        ctx.SaveChanges();
    }

    public void SaveAll(string accountId, IEnumerable<AccountTransaction> transactions)
    {
        using var ctx = _factory.CreateDbContext();

        // Load existing TxIds for this account so we can diff efficiently
        var existingIds = ctx.Transactions
            .Where(t => t.AccountId == accountId)
            .Select(t => t.TxId)
            .ToHashSet();

        foreach (var tx in transactions)
        {
            if (existingIds.Contains(tx.TxId))
                UpdateTx(ctx, tx);
            else
                ctx.Transactions.Add(ToEntity(accountId, tx));
        }

        ctx.SaveChanges();
    }

    public void DeleteByAccount(string accountId)
    {
        using var ctx = _factory.CreateDbContext();
        var txs = ctx.Transactions.Where(t => t.AccountId == accountId).ToList();
        ctx.Transactions.RemoveRange(txs);   // cascade deletes postings
        ctx.SaveChanges();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void UpsertTx(BankingDbContext ctx, string accountId, AccountTransaction tx)
    {
        var existing = ctx.Transactions.Include(t => t.Postings)
                          .FirstOrDefault(t => t.TxId == tx.TxId);
        if (existing is null)
            ctx.Transactions.Add(ToEntity(accountId, tx));
        else
            UpdateTx(ctx, tx, existing);
    }

    private static void UpdateTx(BankingDbContext ctx, AccountTransaction tx,
                                  TransactionEntity? entity = null)
    {
        entity ??= ctx.Transactions.Include(t => t.Postings)
                      .FirstOrDefault(t => t.TxId == tx.TxId);
        if (entity is null) return;

        entity.Date           = tx.Date;
        entity.Amount         = tx.Amount;
        entity.Currency       = tx.Currency;
        entity.BalanceAddress = tx.BalanceAddress;
        entity.Type           = tx.Type;
        entity.Status         = tx.Status;
        entity.Description    = tx.Description;

        // Sync postings: remove stale, add new
        var newIds = tx.Postings.Select(p => p.PostingId).ToHashSet();
        entity.Postings.RemoveAll(p => !newIds.Contains(p.PostingId));
        var existingPostingIds = entity.Postings.Select(p => p.PostingId).ToHashSet();
        foreach (var p in tx.Postings.Where(p => !existingPostingIds.Contains(p.PostingId)))
            entity.Postings.Add(ToEntity(p, tx.TxId));
    }

    // ── Mappers ───────────────────────────────────────────────────────────────

    private static TransactionEntity ToEntity(string accountId, AccountTransaction tx) => new()
    {
        TxId           = tx.TxId,
        AccountId      = accountId,
        Date           = tx.Date,
        Amount         = tx.Amount,
        Currency       = tx.Currency,
        BalanceAddress = tx.BalanceAddress,
        Type           = tx.Type,
        Status         = tx.Status,
        Description    = tx.Description,
        Postings       = tx.Postings.Select(p => ToEntity(p, tx.TxId)).ToList(),
    };

    private static PostingEntity ToEntity(Posting p, string txId) => new()
    {
        PostingId   = p.PostingId,
        TxId        = txId,
        Date        = p.Date,
        DrAddress   = p.DrAddress,
        CrAddress   = p.CrAddress,
        Amount      = p.Amount,
        Currency    = p.Currency,
        Description = p.Description,
    };

    private static AccountTransaction ToDomain(TransactionEntity e) => new()
    {
        TxId           = e.TxId,
        Date           = e.Date,
        Amount         = e.Amount,
        Currency       = e.Currency,
        BalanceAddress = e.BalanceAddress,
        Type           = e.Type,
        Status         = e.Status,
        Description    = e.Description,
        Postings       = e.Postings.OrderBy(p => p.PostingId)
                                   .Select(ToDomain).ToList(),
    };

    private static Posting ToDomain(PostingEntity e) => new()
    {
        PostingId   = e.PostingId,
        TxId        = e.TxId,
        Date        = e.Date,
        DrAddress   = e.DrAddress,
        CrAddress   = e.CrAddress,
        Amount      = e.Amount,
        Currency    = e.Currency,
        Description = e.Description,
    };
}
