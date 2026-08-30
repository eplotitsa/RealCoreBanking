namespace RealCoreBanking.Core.Events;

public abstract record BankEvent : IBankEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}

public record DayOpenedEvent(DateTime BusinessDate) : BankEvent
{
    public override string EventType => "DayOpened";
}

public record DayClosedEvent(DateTime BusinessDate) : BankEvent
{
    public override string EventType => "DayClosed";
}

public record AccountOpenedEvent(string AccountNumber) : BankEvent
{
    public override string EventType => "AccountOpened";
}

public record AccountClosedEvent(string AccountNumber) : BankEvent
{
    public override string EventType => "AccountClosed";
}

public record AccountStatusChangedEvent(string AccountNumber, string OldStatus, string NewStatus) : BankEvent
{
    public override string EventType => "AccountStatusChanged";
}

public record ProductChangedEvent(string AccountNumber, string OldProduct, string NewProduct) : BankEvent
{
    public override string EventType => "ProductChanged";
}

public record CreditBlockedEvent(string AccountNumber) : BankEvent
{
    public override string EventType => "CreditBlocked";
}

public record DebitBlockedEvent(string AccountNumber) : BankEvent
{
    public override string EventType => "DebitBlocked";
}

public record DepositEvent(string AccountNumber, decimal Amount, long BalanceAddress) : BankEvent
{
    public override string EventType => "Deposit";
}

public record WithdrawEvent(string AccountNumber, decimal Amount, long BalanceAddress) : BankEvent
{
    public override string EventType => "Withdraw";
}
