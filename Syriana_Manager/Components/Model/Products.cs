using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Syriana_Manager.Components.Model
{
    public class Products
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name ist erforderlich.")]
        [StringLength(100)]
        public string? Name_de { get; set; }
        [Required(ErrorMessage = "Description ist erforderlich.")]
        [StringLength(255)]
        public string? Description_de { get; set; }
        [Required(ErrorMessage = "Bitte wählen Sie eine Kategorie aus.")]
        public int CategoryId { get; set; }
        public Categories? Category { get; set; }
        public string? Barcode { get; set; } = "BarcodeNull";
        public int Quantity { get; set; } = 50;
        [Range(0.01, double.MaxValue, ErrorMessage = "Purchase Price muss größer als 0 sein.")]
        public double PurchasePrice { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Sale Price muss größer als 0 sein.")]
        public double SalePrice { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Minimum Stock muss größer als 0 sein.")]
        public int MinimumStock { get; set; } = 10;
        [Required(ErrorMessage = "Startdatum ist erforderlich.")]
        [DateInFuture(ErrorMessage = "Startdatum darf nicht in der Vergangenheit liegen.")]
        public DateTime EXPDate { get; set; } = DateTime.Now.AddYears(2);

        public ICollection<Suppliers> Suppliers { get; set; } = [];
        [NotMapped]
        [Required(ErrorMessage = "Bitte wählen Sie mindestens einen Lieferanten aus.")] 
        [MinLength(1, ErrorMessage = "Mindestens ein Lieferant ist erforderlich.")] 
        public List<int> SelectedSupplierIds { get; set; } = [];
        public int UserId { get; set; }
        public Users? User { get; set; }
        public ICollection<ProductImages> ProductImages { get; set; } = [];
        [Required(ErrorMessage = "يجب ادخال اسم المنتج ايضا بل عربية")]
        [StringLength(100)]
        public string? Name_ar { get; set; }
        [Required(ErrorMessage = "يجب ادخال وصف المنتج ايضا بل عربية")]
        [StringLength(255)]
        public string? Description_ar { get; set; }
        [Required(ErrorMessage = "Bitte geben Sie die Steuersatz ein")]
        public int TaxRateId { get; set; }
        public TaxRate? TaxRate { get; set; }
        public int? ProductGroupID { get; set; }
        public GroupProducts? ProductGroup { get; set; }
        public bool IsShippable { get; set; } = true;
        public ProductDiscounts? ProductDiscount { get; set; } = new();
        public PackagingUnits PackagingUnit { get; set; } = PackagingUnits.Piece;
        public int ItemsPerPackage { get; set; } = 1;

        //
        public CartItem CartItem { get; set; } = null!;
        public void InitializeCartItem(int quantity)
        {
            CartItem = new CartItem
            {
                ProductId = this.Id, // تأكد أن Id معروف
                Quantity = quantity,
                Product = this! 
            };
        }

        // time validation attribute for future dates
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
    public enum PackagingUnits
    {
        [Display(Name = "Piece")]
        Piece = 1,
        [Display(Name = "Kilogram")]
        Kilogram = 2,
        [Display(Name = "Box")]
        Box = 3,
        [Display(Name = "Sack")]
        Sack = 4,
        [Display(Name = "Crate")]
        Crate = 5
    }
}
