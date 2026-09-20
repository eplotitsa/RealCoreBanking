using RealCoreBanking.Core.Models;

namespace RealCoreBanking.Data.Repositories;

public interface ITransactionRepository
{
    /// <summary>Return all transactions (with postings) for a given account.</summary>
    IReadOnlyList<AccountTransaction> GetByAccount(string accountId);

    /// <summary>Persist a new transaction and its postings (upsert).</summary>
    void Save(string accountId, AccountTransaction tx);

    /// <summary>Persist multiple transactions atomically (used on bulk account save).</summary>
    void SaveAll(string accountId, IEnumerable<AccountTransaction> transactions);

    /// <summary>Delete all transactions for an account (used when account is deleted).</summary>
    void DeleteByAccount(string accountId);
}
