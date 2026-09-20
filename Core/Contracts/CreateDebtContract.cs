using RealCoreBanking.Core.Models;

namespace RealCoreBanking.Core.Contracts;

public class CreateDebtContract : ISmartContract
{
    private readonly Product _product;

    public string Name    => "СК: Создание долга";
    public bool   Success { get; private set; } = true;

    public CreateDebtContract(Product product) => _product = product;

    public Task ExecuteAsync(CurrentAccount account, CancellationToken ct = default)
    {
        account.DebtArray ??= new DebtArray();

        var billingDate = account.BillingDate?.Date ?? DateTime.Today;
        var dueDate     = account.DueDate?.Date     ?? DateTime.Today;

        int order = 0;
        foreach (var debtRef in _product.DebtBalanceAddresses)
        {
            var balance = account.Balances.GetByAddress(debtRef.Address, debtRef.Currency);
            if (balance == null || balance.Amount <= 0) { order++; continue; }

            account.DebtArray.Debts.Add(new DebtComponent
            {
                Order       = order,
                Address     = debtRef.Address,
                Currency    = balance.Currency,
                BillingDate = billingDate,
                DueDate     = dueDate,
                Amount      = balance.Amount,
            });
            order++;
        }

        return Task.CompletedTask;
    }
}
