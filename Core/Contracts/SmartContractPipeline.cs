using RealCoreBanking.Core.Models;

namespace RealCoreBanking.Core.Contracts;

/// <summary>
/// Orchestrates smart contract execution for a single account.
/// BeforeChange runs in parallel across all contracts (read-only validation).
/// Execute + AfterChange run sequentially (state mutation).
/// </summary>
public class SmartContractPipeline
{
    public async Task RunAsync(
        CurrentAccount account,
        IEnumerable<ISmartContract> contracts,
        CancellationToken ct = default)
    {
        var list = contracts.ToList();

        // Validation phase: parallel — no state mutation
        await Task.WhenAll(list.Select(c => c.BeforeChangeAsync(account, ct)));

        // Mutation phase: sequential — order matters
        foreach (var c in list)
        {
            await c.ExecuteAsync(account, ct);
            await c.AfterChangeAsync(account, ct);
        }
    }

    /// <summary>
    /// Run one contract across many accounts in parallel (e.g. day-close interest accrual).
    /// </summary>
    public Task RunOnManyAccountsAsync(
        ISmartContract contract,
        IEnumerable<CurrentAccount> accounts,
        CancellationToken ct = default)
        => Task.WhenAll(accounts.Select(a => RunAsync(a, [contract], ct)));
}
