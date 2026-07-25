using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Syriana_Manager.Components.Model
{
    public class ProductImages
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        [NotMapped]
        [JsonIgnore]
        public string ImageUrlLocal { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public int ProductId { get; set; }
        public byte[]? ImageBytes { get; set; }
        public DateTime LastModified { get; set; }
        public Products? Product { get; set; }

    }
}
