using System.ComponentModel.DataAnnotations;

namespace Syriana_Manager.Components.Model
{
    public class GroupProducts
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name de ist erforderlich.")]
        public string GroupName_de { get; set; } = string.Empty;
        [Required(ErrorMessage = "Name ar ist erforderlich.")]
        public string GroupName_ar { get; set; } = string.Empty;
    }
}
