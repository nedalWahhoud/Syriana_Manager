using System.Text.Json.Serialization;

namespace Syriana_Manager.Components.Model
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 0;
        // Ignore the products if they will convert to json
        [JsonIgnore]
        public Products Product { get; set; } = null!;
    }
}
