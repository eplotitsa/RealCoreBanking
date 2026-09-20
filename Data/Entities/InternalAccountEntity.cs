using System.ComponentModel.DataAnnotations;

namespace RealCoreBanking.Data.Entities;

public class InternalAccountEntity
{
    [Key] public string AccountId { get; set; } = "";
    public string Data { get; set; } = "{}";
}
