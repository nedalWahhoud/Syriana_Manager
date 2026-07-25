using Syriana_Manager.Components.Model;
namespace Syriana_Manager.Components.Model
{
    public class OrderItems
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public Products? Product { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public int CategoryId { get; set; }
    }
}
