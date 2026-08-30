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

        foreach (var debtRef in _product.DebtBalanceAddresses)
        {
            var balance = account.Balances.GetByAddress(debtRef.Address, debtRef.Currency);
            if (balance == null || balance.Amount <= 0) continue;

            account.DebtArray.Debts.Add(new DebtComponent
            {
                FinancialIndicatorCode = balance.FinancialIndicatorCode,
                Amount                 = balance.Amount,
                Currency               = balance.Currency,
                EntryDate              = DateTime.Today,
                Description            = balance.Name,
            });
        }
    }
}
