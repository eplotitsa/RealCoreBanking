namespace RealCoreBanking.Core.Models;

public class AccountTransaction
{
    public string   TxId           { get; set; } = "";
    public DateTime Date           { get; set; }
    public decimal  Amount         { get; set; }
    public string   Currency       { get; set; } = "";
    public long     BalanceAddress { get; set; }
    /// <summary>Financial (requires posting) or NonFinancial (balance change only)</summary>
    public string   Type           { get; set; } = "Financial";
    /// <summary>Posted / Rejected</summary>
    public string   Status         { get; set; } = "Posted";
    public string   Description    { get; set; } = "";
    public List<Posting> Postings  { get; set; } = new();
}
