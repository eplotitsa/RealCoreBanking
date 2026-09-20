namespace RealCoreBanking.Core.Models;

public class Posting
{
    public string   PostingId   { get; set; } = "";
    public string   TxId        { get; set; } = "";
    public DateTime Date        { get; set; }
    public long     DrAddress   { get; set; }   // 0 = external counterparty
    public long     CrAddress   { get; set; }   // 0 = external counterparty
    public decimal  Amount      { get; set; }
    public string   Currency    { get; set; } = "";
    public string   Description { get; set; } = "";
}
