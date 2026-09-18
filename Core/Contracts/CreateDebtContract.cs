using RealCoreBanking.Core.Models;

namespace RealCoreBanking.Core.Contracts;

public class CreateDebtContract : ISmartContract
{
    private readonly Product _product;

    public string Name => "СК: Создание долга";

    public CreateDebtContract(Product product) => _product = product;

    public void Execute(CurrentAccount account)
    {
        account.DebtArray ??= new DebtArray();

        var billingDate = account.BillingDate?.Date ?? DateTime.Today;
        var dueDate     = account.DueDate?.Date     ?? DateTime.Today;

        // Order follows the sequence of DebtBalanceAddresses in the product
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
    }
}
