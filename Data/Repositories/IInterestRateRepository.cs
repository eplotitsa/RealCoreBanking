using RealCoreBanking.Core.Models;

namespace RealCoreBanking.Data.Repositories;

/// <summary>
/// Persistence contract for InterestRate.
/// Implementations: EfInterestRateRepository (EF Core/SQLite/Postgres/SQL Server).
/// </summary>
public interface IInterestRateRepository
{
    /// <summary>Load all rates (called once on startup).</summary>
    IReadOnlyList<InterestRate> GetAll();

    /// <summary>Insert or update a rate by Code.</summary>
    void Save(InterestRate rate);

    /// <summary>Remove a rate by Code.</summary>
    void Delete(int code);
}
