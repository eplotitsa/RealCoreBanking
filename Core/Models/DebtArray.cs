using RealCoreBanking.Core.Enums;

namespace RealCoreBanking.Core.Models;

public class DebtArray
{
    public int DaysPastDue { get; set; }
    public RepaymentScheme RepaymentScheme { get; set; } = RepaymentScheme.Vertical;
    public int ContinuousDelinquencyDays { get; set; }

    public List<DebtComponent> Debts { get; set; } = new();
    public List<RepaidPayment> RepaidPayments { get; set; } = new();

    public decimal TotalDebt => Debts.Sum(d => d.Amount);

    public decimal GetDebtByFinancialCode(string fiCode) =>
        Debts.Where(d => d.FinancialIndicatorCode == fiCode).Sum(d => d.Amount);

    public IEnumerable<DebtComponent> GetAllDebts() => Debts.OrderBy(d => d.EntryDate);

    public void CalculateDpd(DateTime businessDate)
    {
        if (!Debts.Any()) { DaysPastDue = 0; return; }
        var oldest = Debts.Min(d => d.EntryDate);
        DaysPastDue = Math.Max(0, (int)(businessDate - oldest).TotalDays);
    }

    public void CalculateContinuousDelinquency(DateTime businessDate)
    {
        if (!Debts.Any()) { ContinuousDelinquencyDays = 0; return; }
        var firstEntry = Debts.Min(d => d.EntryDate);
        ContinuousDelinquencyDays = Math.Max(0, (int)(businessDate - firstEntry).TotalDays);
    }

    public bool DistributePayment(decimal amount, string currency)
    {
        var remaining = amount;
        List<DebtComponent> targets;

        if (RepaymentScheme == RepaymentScheme.Vertical)
        {
            targets = Debts
                .Where(d => d.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase))
                .OrderBy(d => d.EntryDate)
                .ToList();

            foreach (var debt in targets)
            {
                if (remaining <= 0) break;
                var paid = Math.Min(remaining, debt.Amount);
                debt.Amount -= paid;
                remaining -= paid;
                RepaidPayments.Add(new RepaidPayment
                {
                    FinancialIndicatorCode = debt.FinancialIndicatorCode,
                    EntryDate              = debt.EntryDate,
                    RepaymentDate          = DateTime.Today,
                    Amount                 = paid,
                    Currency               = currency,
                });
            }
        }
        else // Horizontal — proportional
        {
            targets = Debts
                .Where(d => d.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase) && d.Amount > 0)
                .ToList();

            var total = targets.Sum(d => d.Amount);
            if (total <= 0) return true;

            foreach (var debt in targets)
            {
                var share = Math.Min(remaining, Math.Round(amount * debt.Amount / total, 2));
                debt.Amount -= share;
                remaining -= share;
                RepaidPayments.Add(new RepaidPayment
                {
                    FinancialIndicatorCode = debt.FinancialIndicatorCode,
                    EntryDate              = debt.EntryDate,
                    RepaymentDate          = DateTime.Today,
                    Amount                 = share,
                    Currency               = currency,
                });
            }
        }

        Debts.RemoveAll(d =>
            d.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase) && d.Amount <= 0);

        return remaining <= 0;
    }
}
