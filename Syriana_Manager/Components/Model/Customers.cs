using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Syriana_Manager.Components.Model
{
    public class Customers
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name de ist erforderlich.")]
        public string Name_de { get; set; } = string.Empty;
        [Required(ErrorMessage = "Name ar ist erforderlich.")]
        public string Name_ar { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string BuildingNumber { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        [Required(ErrorMessage = "Latitude ist erforderlich.")]
        [Range(-90, 90, ErrorMessage = "Latitude ist ungültig.")]
        public double Latitude { get; set; }
        [Required(ErrorMessage = "Longitude ist erforderlich.")]
        [Range(-180, 180, ErrorMessage = "Longitude ist ungültig.")]
        public double Longitude { get; set; }
        [RegularExpression(@"^\+?[0-9\s\-]{7,20}$",
         ErrorMessage = "Telefonnummer ist ungültig.")]
        public string? PhoneNumber { get; set; }
        [RegularExpression(@"^$|^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "E-Mail ist ungültig." )]
        public string? Email { get; set; }
        [StringLength(200, ErrorMessage = "Note darf maximal 200 Zeichen lang sein.")]
        public string? Notes_de { get; set; }
        [StringLength(200, ErrorMessage = "Note darf maximal 200 Zeichen lang sein.")]
        public string? Notes_ar { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int StopNumber { get; set; } = 1;
        public bool shouldStopnummerShift { get; set; } = false;
        [RegularExpression(@"^\d{4}$", ErrorMessage = "PIN muss nur aus 4 Zahlen bestehen.")]
        public string? PIN { get; set; }
        public bool HasOneTimePaymentToday { get; set; } = false;
        public bool HasDebt {  get; set; } = false;
        // 🔗 FK
        [Range(1, int.MaxValue, ErrorMessage = "DistributionLineId muss angegeben werden.")]
        public int DistributionLineId { get; set; }
        public DistributionLines? DistributionLine { get; set; }

        public virtual ICollection<OneTimePayment> OneTimePayments { get; set; } = [];
    }
    public class CustomerProcess
    {
        public int CustomerId { get; set; }
        public bool IsProcessing { get; set; } = false;
        public ValidationResult Result { get; set; } = null!;
        public Customers EditingCustomer { get; set; } = null!; 
        public bool IsMapOpen { get; set; } = false;
    }
}
