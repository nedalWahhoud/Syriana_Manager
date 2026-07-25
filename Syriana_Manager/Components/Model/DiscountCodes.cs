using System.ComponentModel.DataAnnotations;

namespace Syriana_Manager.Components.Model
{
    public class DiscountCodes
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name ist erforderlich.")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "Code muss genau 8 Zeichen lang sein.")]
        public string Code { get; set; } = string.Empty;
        [Range(1, int.MaxValue, ErrorMessage = "Wert muss größer als 0 sein.")]
        public int DiscountAmount { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Wert muss größer als 0 sein.")]
        public int UsageLimit { get; set; }
        public int TimesUsed { get; set; }
        [Required(ErrorMessage = "Startdatum ist erforderlich.")]
        [DateInFuture(ErrorMessage = "Startdatum muss in der Zukunft liegen.")]
        public DateTime StartDate { get; set; } = DateTime.Today;
        [Required(ErrorMessage = "Enddatum ist erforderlich.")]
        [DateInFuture(ErrorMessage = "Enddatum muss in der Zukunft liegen.")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(30);
        public bool IsActive { get; set; } = true;
        [Required(ErrorMessage = "Rabatttyp ist erforderlich.")]
        public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

        public string Message { get; set; } = string.Empty;
    }
    public class DateInFutureAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is DateTime date)
            {
                return date.Date >= DateTime.Today;
            }
            return false;
        }
    }
}
