using Microsoft.Extensions.Options;

namespace Syriana_Manager.Components.Model
{
    public class Invoice
    {
        public string InvoceeNumber { get; set; } = string.Empty;
        public Order CurrentOrder { get; set; } = new();
    }
}
