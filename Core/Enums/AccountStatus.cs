namespace RealCoreBanking.Core.Enums;

// Extensible status — new statuses can be registered at runtime
public sealed class AccountStatus
{
    public string Value { get; }

    private AccountStatus(string value) => Value = value;

    public static readonly AccountStatus Open               = new("open");
    public static readonly AccountStatus Sleeping           = new("sleeping");
    public static readonly AccountStatus Dormant            = new("dormant");
    public static readonly AccountStatus Closed             = new("close");
    public static readonly AccountStatus WaitingForApproval = new("waiting for approval");

    private static readonly List<AccountStatus> _all = new() { Open, Sleeping, Dormant, Closed, WaitingForApproval };
    public static IReadOnlyList<AccountStatus> All => _all.AsReadOnly();

    public static AccountStatus Register(string value)
    {
        var existing = _all.FirstOrDefault(s => s.Value.Equals(value, StringComparison.OrdinalIgnoreCase));
        if (existing != null) return existing;
        var status = new AccountStatus(value);
        _all.Add(status);
        return status;
    }

    public static AccountStatus? FromString(string value) =>
        _all.FirstOrDefault(s => s.Value.Equals(value, StringComparison.OrdinalIgnoreCase));

    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is AccountStatus s && s.Value == Value;
    public override int GetHashCode() => Value.GetHashCode();
}
