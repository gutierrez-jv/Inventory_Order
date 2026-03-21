using System.ComponentModel.DataAnnotations;

namespace Inventory_Order.ViewModels.Order
{
    public class OrderFormViewModel
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Order ID must be greater than 0.")]
        [Display(Name = "Order ID")]
        public int OrdersId { get; set; }

        [Required]
        [Display(Name = "Customer")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a customer.")]
        public int CustomersId { get; set; }

        [Required]
        [Display(Name = "Product")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a product.")]
        public int ProductsId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
