namespace RealCoreBanking.Core.Enums;

public sealed class FinancialIndicator
{
    public string Code { get; }
    public string Description { get; private set; }

    private FinancialIndicator(string code, string description)
    {
        Code = code;
        Description = description;
    }

    // Convenience references — populated once AccountStore seeds from DB.
    public static readonly FinancialIndicator MainAmount      = new("001", "Основная сумма");
    public static readonly FinancialIndicator AccruedInterest = new("002", "Проценты начисленные");
    public static readonly FinancialIndicator AccruedLosses   = new("003", "Начисленные убытки");
    public static readonly FinancialIndicator OffBalance      = new("004", "Внебаланс");
    public static readonly FinancialIndicator Blocking        = new("005", "Блокировка");
    public static readonly FinancialIndicator OverdraftLimit  = new("006", "Лимит овердрафта");
    public static readonly FinancialIndicator UsedOverdraft   = new("007", "Сумма использованного овердрафта");
    public static readonly FinancialIndicator LatePaymentFee  = new("008", "Штраф за просрочку");
    public static readonly FinancialIndicator Debt            = new("009", "Долг");

    // Populated at startup from DB (empty until AccountStore seeds it).
    private static readonly List<FinancialIndicator> _all = new();

    public static IReadOnlyList<FinancialIndicator> All => _all.AsReadOnly();

    public static FinancialIndicator Register(string code, string description)
    {
        var existing = _all.FirstOrDefault(f => f.Code == code);
        if (existing != null)
        {
            existing.Description = description;
            return existing;
        }
        var fi = new FinancialIndicator(code, description);
        _all.Add(fi);
        return fi;
    }

    public static void Unregister(string code) =>
        _all.RemoveAll(f => f.Code == code);

    public static FinancialIndicator? FromCode(string code) =>
        _all.FirstOrDefault(f => f.Code == code);

    public override string ToString() => $"{Code} — {Description}";
}
