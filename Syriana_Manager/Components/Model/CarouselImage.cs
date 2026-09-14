using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace Syriana_Manager.Components.Model
{
    public class CarouselImage
    {
        public int Id { get; set; }

        [NotMapped]
        [JsonIgnore]
        public string ImageUrlLocal { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public byte[]? ImageBytes { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now.AddDays(0);
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(7);
        public int DisplayOrder { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
