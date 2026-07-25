namespace Syriana_Manager.Components.Model
{
    public class ShippingProvider
    {
        public int Id { get; set; }
        public string Name_de { get; set; } = string.Empty;
        public string Name_ar { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public double PublicShippingCost { get; set; }
        public double FreeShippingThreshold { get; set; }
    }
}
