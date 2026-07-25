using System.ComponentModel.DataAnnotations;

namespace Syriana_Manager.Components.Model
{
    public class Address
    {
        public int Id { get; set; }
        /*[Required(ErrorMessage = "Vorname ist erforderlich.")]
        [RegularExpression(@"^[a-zA-ZäöüÄÖÜß\s\-']{2,50}$", ErrorMessage = "Bitte geben Sie einen gültigen Vornamen ein.")]*/
        public string FirstName { get; set; } = string.Empty;
        /*[Required(ErrorMessage = "Nachname ist erforderlich.")]
        [RegularExpression(@"^[a-zA-ZäöüÄÖÜß\s\-']{2,50}$", ErrorMessage = "Bitte geben Sie einen gültigen Nachnamen ein.")]*/
        public string LastName { get; set; } = string.Empty;
        /*[Required(ErrorMessage = "Telefonnummer ist erforderlich.")]
        [RegularExpression(@"^\+?[0-9\s\-]{7,20}$", ErrorMessage = "Bitte geben Sie eine gültige Telefonnummer ein.")]*/
        public string Phone { get; set; } = string.Empty;
        /*[Required(ErrorMessage = "Straße und Hausnummer ist erforderlich.")]
        [RegularExpression(@"^.+\s+\d+.*$", ErrorMessage = "Bitte geben Sie eine gültige Straße mit Hausnummer ein.")]*/
        public string Street { get; set; } = string.Empty;
        /*[Required(ErrorMessage = "PLZ ist erforderlich.")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Bitte geben Sie eine gültige 5-stellige PLZ ein.")]*/
        public string ZipCode { get; set; } = string.Empty;
        /*[Required(ErrorMessage = "Stadt ist erforderlich.")]
        [RegularExpression(@"^[a-zA-ZäöüÄÖÜß\s\-]{2,50}$", ErrorMessage = "Bitte geben Sie einen gültigen Stadtnamen ein.")]*/
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = "DE";
        public int? UserId { get; set; }
    }
}
