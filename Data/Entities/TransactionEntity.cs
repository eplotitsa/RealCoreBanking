using System.ComponentModel.DataAnnotations;

namespace RealCoreBanking.Data.Entities;

/// <summary>
/// One account transaction (financial or non-financial).
/// AccountId links back to CurrentAccounts or InternalAccounts.
/// </summary>
public class TransactionEntity
{
    [Key] public string   TxId           { get; set; } = "";
    public string         AccountId      { get; set; } = "";
    public DateTime       Date           { get; set; }
    public decimal        Amount         { get; set; }
    public string         Currency       { get; set; } = "";
    public long           BalanceAddress { get; set; }
    public string         Type           { get; set; } = "Financial";
    public string         Status         { get; set; } = "Posted";
    public string         Description    { get; set; } = "";

    // navigation
    public List<PostingEntity> Postings  { get; set; } = new();
}

/// <summary>
/// One double-entry posting line belonging to a transaction.
/// </summary>
public class PostingEntity
{
    [Key] public string PostingId   { get; set; } = "";
    public string       TxId        { get; set; } = "";
    public DateTime     Date        { get; set; }
    public long         DrAddress   { get; set; }
    public long         CrAddress   { get; set; }
    public decimal      Amount      { get; set; }
    public string       Currency    { get; set; } = "";
    public string       Description { get; set; } = "";

    // navigation (FK)
    public TransactionEntity? Transaction { get; set; }
}
