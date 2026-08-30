using RealCoreBanking.Core.Models;

namespace RealCoreBanking.Data.Repositories;

/// <summary>
/// Persistence contract for RateIndex and its value history.
/// Values are append-only (can only be added or voided, never edited).
/// </summary>
public interface IRateIndexRepository
{
    /// <summary>Load all indexes including full value history (called once on startup).</summary>
    IReadOnlyList<RateIndex> GetAll();

    /// <summary>Insert or update index metadata (not values).</summary>
    void Save(RateIndex index);

    /// <summary>Remove an index and all its values (cascade).</summary>
    void Delete(string code);

    /// <summary>
    /// Append a new published value.
    /// Automatically supersedes any existing Active value on the same ValueDate.
    /// </summary>
    void AddValue(string indexCode, RateValue value);

    /// <summary>Mark an Active value as Voided.</summary>
    void VoidValue(string indexCode, DateTime valueDate);
}
