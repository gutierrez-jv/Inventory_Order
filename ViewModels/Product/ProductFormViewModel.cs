using System.ComponentModel.DataAnnotations;

namespace Inventory_Order.ViewModels.Product
{
    public class ProductFormViewModel
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Product ID must be greater than 0.")]
        [Display(Name = "Product ID")]
        public int ProductsId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Product Type")]
        public string Type { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue)]
        [Display(Name = "Stock Quantity")]
        public int Quantity { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        [Display(Name = "Price per Item")]
        public int Price { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Product Code / Barcode")]
        public string Barcode { get; set; } = string.Empty;
    }
}
