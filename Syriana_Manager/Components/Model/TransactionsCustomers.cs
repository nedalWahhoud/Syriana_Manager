using System.Text.Json.Serialization;

namespace Syriana_Manager.Components.Model
{
    public class TransactionsCustomers
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public TransactionType Type { get; set; } 
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Notes { get; set; }
        [JsonIgnore]
        public decimal AmountInput { get; set; }
        [JsonIgnore]
        public decimal DraftAmount { get; set; }
        [JsonIgnore]
        public TransactionType? DraftType { get; set; }
    }
    public enum TransactionType
    {
        Borrow,
        Repay
    }
}
