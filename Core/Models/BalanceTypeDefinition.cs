namespace RealCoreBanking.Core.Models;

/// <summary>
/// Catalog of balance types (DB-backed). Code is 2 chars ("01"–"05").
/// Populated at startup by AccountStore from the BalanceTypes table.
/// </summary>
public sealed class BalanceTypeDefinition
{
    public string Code { get; }
    public string Description { get; private set; }

    private BalanceTypeDefinition(string code, string description)
    {
        Code = code;
        Description = description;
    }

    private static readonly List<BalanceTypeDefinition> _all = new();

    public static IReadOnlyList<BalanceTypeDefinition> All => _all.AsReadOnly();

    public static BalanceTypeDefinition Register(string code, string description)
    {
        var existing = _all.FirstOrDefault(t => t.Code == code);
        if (existing != null)
        {
            existing.Description = description;
            return existing;
        }
        var bt = new BalanceTypeDefinition(code, description);
        _all.Add(bt);
        return bt;
    }

    public static void Unregister(string code) =>
        _all.RemoveAll(t => t.Code == code);

    public static BalanceTypeDefinition? FromCode(string code) =>
        _all.FirstOrDefault(t => t.Code == code);

    /// <summary>Look up by the integer prefix used in BalanceDefinition (1 → "01", 2 → "02", …).</summary>
    public static BalanceTypeDefinition? FromPrefix(int prefix) =>
        FromCode(prefix.ToString("D2"));

    public override string ToString() => $"{Code} — {Description}";
}
