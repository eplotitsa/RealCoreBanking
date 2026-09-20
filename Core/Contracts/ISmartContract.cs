using RealCoreBanking.Core.Models;

namespace RealCoreBanking.Core.Contracts;

public interface ISmartContract
{
    string Name    { get; }
    bool   Success { get; }

    Task BeforeChangeAsync(CurrentAccount account, CancellationToken ct = default)
        => Task.CompletedTask;

    Task ExecuteAsync(CurrentAccount account, CancellationToken ct = default);

    Task AfterChangeAsync(CurrentAccount account, CancellationToken ct = default)
        => Task.CompletedTask;

    Task OnDayOpenAsync(CurrentAccount account, CancellationToken ct = default)
        => Task.CompletedTask;

    Task OnDayCloseAsync(CurrentAccount account, CancellationToken ct = default)
        => Task.CompletedTask;
}
