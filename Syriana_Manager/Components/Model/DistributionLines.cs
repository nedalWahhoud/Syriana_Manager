using System.ComponentModel.DataAnnotations;

namespace Syriana_Manager.Components.Model
{
    public class DistributionLines
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "LineName ist erforderlich.")]
        public string LineName { get; set; } = string.Empty;
        [StringLength(200, ErrorMessage = "Beschreibung darf maximal 200 Zeichen lang sein.")]
        public string Description { get; set; } = string.Empty;
    }
}
