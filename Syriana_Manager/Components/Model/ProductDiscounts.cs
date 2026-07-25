using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Syriana_Manager.Components.Model
{
    public class ProductDiscounts
    {
        public int Id { get; set; }
        public int ProductsId { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Der Preis darf nicht negativ sein.")]
        public double DiscountedPrice { get; set; }
        [NotInPast(ErrorMessage = "Startdatum darf nicht in der Vergangenheit liegen.")]
        public DateTime StartDate { get; set; } = DateTime.Today;
        [NotInPast(ErrorMessage = "Startdatum darf nicht in der Vergangenheit liegen.")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(30);
        [JsonIgnore]
        public Products? Product { get; set; }


    }

    public class NotInPastAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is DateTime date)
                return date.Date >= DateTime.Today;

            return true;
        }
    }
}
