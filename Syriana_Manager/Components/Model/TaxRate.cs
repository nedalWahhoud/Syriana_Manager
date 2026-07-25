namespace Syriana_Manager.Components.Model
{
    public class TaxRate
    {
        public int Id { get; set; }
        public double Rate { get; set; }
        public string Description_de { set; get; } = string.Empty;
        public string Description_ar { set; get; } = string.Empty;

    }
}
