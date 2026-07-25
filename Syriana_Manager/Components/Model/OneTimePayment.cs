using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace Syriana_Manager.Components.Model
{
    public class OneTimePayment
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Bitte Kunde auswählen.")]
        public int CustomerId { get; set; }
        [Required(ErrorMessage = "Bitte Line auswählen.")]
        public int DistributionLineId { get; set; }
        [Required(ErrorMessage = "Bitte geben Sie den geforderten Gesamtbetrag ein.")]
        [Range(0.01, 999999.99, ErrorMessage = "Der Betrag darf nicht 0 sein.")]
        public double TotalAmount { get; set; }
        public double AmountCollected { get; set; }
        [Required(ErrorMessage = "Bitte wählen Sie einen Status für die Zahlung aus.")]
        public OneTimePaymentStatus Status { get; set; } = OneTimePaymentStatus.Offen;
        [StringLength(500)]
        public string? Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
        [Required(ErrorMessage = "Bitte geben Sie ein Abholdatum an.")]
        [DateInFuture(ErrorMessage = "Startdatum darf nicht in der Vergangenheit liegen.")]
        public DateTime PickupDate { get; set; } = DateTime.Now.AddDays(1);

        [ForeignKey("CustomerId")]
        public virtual Customers? Customer { get; set; }
        [ForeignKey("DistributionLineId")]
        public virtual DistributionLines? DistributionLine { get; set; }

        // local nuten
        [JsonIgnore]
        public bool IsProcessing { get; set; } = false; // Hilfsfeld, um anzuzeigen, ob die Zahlung gerade verarbeitet wird
        [JsonIgnore]
        public bool IsProcessingEditingAmount { get; set; } = false;
    }
    public class OneTimePaymentsGroupDto
    {
        public DateTime GroupPickupDate { get; set; } // Datum des ersten Tages in der Gruppe
        public List<OneTimePayment> Payments { get; set; } = [];
    }
    public enum OneTimePaymentStatus
    {
        [Display(Name = "Offen")]
        Offen = 0,
        [Display(Name = "Voll-inkassiert")]
        VollstaendigInkassiert = 1,

        [Display(Name = "Teil-inkassiert")]
        TeilweiseInkassiert = 2,
        [Display(Name = "Überzahlt")]
        Ueberzahlt = 3,
        [Display(Name = "Verschoben")]
        Verschoben = 4
    }
}
