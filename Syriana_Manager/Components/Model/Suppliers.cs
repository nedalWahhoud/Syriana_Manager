using System.ComponentModel.DataAnnotations;

namespace Syriana_Manager.Components.Model
{
    public class Suppliers
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name ist erforderlich.")]
        public string Name { get; set; } = string.Empty;
        public string? Street { get; set; }
        public string? HNumber { get; set; }
        public string? PostalCode { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        [Required(ErrorMessage = "Telefonnummer ist erforderlich.")]
        [RegularExpression(@"^(\+?\d{1,4})?[\d\s\-]{7,15}$", ErrorMessage = "Ungültige Telefonnummer. Nur Zahlen sind erlaubt.")]
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? Name_Ar { get; set; }
    }
}
