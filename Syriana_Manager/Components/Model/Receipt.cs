namespace Syriana_Manager.Components.Model
{
    public class Receipt
    {
        public int Id { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoreAddress { get; set; } = string.Empty;
        public DateTime ReceiptDate { get; set; }
        public List<Products> Products { get; set; } = [];
        public Summe Summe { get; set; } = new Summe();
        public int PaymentMethodId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public List<TaxRates> TaxRates { get; set; } = [];
    }
    public class TaxRates
    {
        public double TaxRate { get; set; }
        public double TaxAmount { get; set; }
        public double NettoPrice { get; set; }
        public double TotalPrice { get; set; }
    }
    public class Summe
    {
        public double TotalTax { get; set; }
        public double NettoPrice { get; set; }
        public double TotalPrice { get; set; }
    }
}
