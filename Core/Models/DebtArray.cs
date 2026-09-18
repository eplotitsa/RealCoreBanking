using RealCoreBanking.Core.Enums;

namespace RealCoreBanking.Core.Models;

public class DebtArray
{
    // Days Past Due — age of the oldest debt component, recalculated on business day open
    public int DaysPastDue { get; set; }

    public RepaymentScheme RepaymentScheme { get; set; } = RepaymentScheme.Vertical;

    // Continuous days in delinquency (uninterrupted)
    public int ContinuousDelinquencyDays { get; set; }

    // FIFO queue: debt components ordered by Order (ascending = repaid first)
    public List<DebtComponent> Debts { get; set; } = new();

    public List<RepaidPayment> RepaidPayments { get; set; } = new();

    public decimal TotalDebt => Debts.Sum(d => d.Amount);

    public decimal GetDebtByAddress(long address) =>
        Debts.Where(d => d.Address == address).Sum(d => d.Amount);

    public IEnumerable<DebtComponent> GetAllDebts() =>
        Debts.OrderBy(d => d.Order).ThenBy(d => d.DueDate);

    public void CalculateDpd(DateTime businessDate)
    {
        if (!Debts.Any()) { DaysPastDue = 0; return; }
        var oldest = Debts.Min(d => d.DueDate);
        DaysPastDue = Math.Max(0, (int)(businessDate - oldest).TotalDays);
    }

    public void CalculateContinuousDelinquency(DateTime businessDate)
    {
        if (!Debts.Any()) { ContinuousDelinquencyDays = 0; return; }
        var first = Debts.Min(d => d.DueDate);
        ContinuousDelinquencyDays = Math.Max(0, (int)(businessDate - first).TotalDays);
    }

    /// <summary>
    /// Distribute a payment across debt components for the given currency.
    /// Vertical: sequential (oldest Order first). Horizontal: proportional.
    /// Returns true if the payment fully covered all debt in that currency.
    /// </summary>
    public bool DistributePayment(decimal amount, string currency, string paymentId = "")
    {
        var remaining = amount;

        var targets = Debts
            .Where(d => d.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase) && d.Amount > 0)
            .OrderBy(d => d.Order)
            .ThenBy(d => d.DueDate)
            .ToList();

        if (RepaymentScheme == RepaymentScheme.Vertical)
        {
            foreach (var debt in targets)
            {
                if (remaining <= 0) break;
                var paid = Math.Min(remaining, debt.Amount);
                debt.Amount -= paid;
                remaining  -= paid;
                RepaidPayments.Add(new RepaidPayment
                {
                    DebtId        = debt.DebtId,
                    PaymentId     = paymentId,
                    RepaymentDate = DateTime.Today,
                    Amount        = paid,
                    Currency      = currency,
                });
            }
        }
        else // Horizontal — proportional
        {
            var total = targets.Sum(d => d.Amount);
            if (total <= 0) return true;

            foreach (var debt in targets)
            {
                var share = Math.Min(remaining, Math.Round(amount * debt.Amount / total, 2));
                debt.Amount -= share;
                remaining  -= share;
                RepaidPayments.Add(new RepaidPayment
                {
                    DebtId        = debt.DebtId,
                    PaymentId     = paymentId,
                    RepaymentDate = DateTime.Today,
                    Amount        = share,
                    Currency      = currency,
                });
            }
        }

        Debts.RemoveAll(d =>
            d.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase) && d.Amount <= 0);

        return remaining <= 0;
    }
}
