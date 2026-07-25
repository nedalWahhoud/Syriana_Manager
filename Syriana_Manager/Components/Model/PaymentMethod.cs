using Syriana_Manager.Components.Model;

namespace Syriana_Manager.Components.Model
{
    public class PaymentMethod
    {
        public int Id { get; set; }
        public string? Method { get; set; }
        public string? Description_de { get; set; }
        public string? Description_ar { get; set; }
        public BankTransferDetails? BankTransferDetails { get; set; } 
    }
}
