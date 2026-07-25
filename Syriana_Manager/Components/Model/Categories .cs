using System.ComponentModel.DataAnnotations;

namespace Syriana_Manager.Components.Model
{
    public class Categories
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name de ist erforderlich.")]
        public string? Name_de { get; set; }
        [Required(ErrorMessage = "Name ar ist erforderlich.")]
        public string? Name_ar { get; set; }
        public bool Requires18Plus { get; set; } = false;
        public bool IsAktiv { get; set; } = true;
    }
}
