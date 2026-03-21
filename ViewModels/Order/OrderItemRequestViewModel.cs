using System.ComponentModel.DataAnnotations;

namespace Inventory_Order.ViewModels.Order
{
    public class OrderItemRequestViewModel
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Product is required.")]
        public int ProductsId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
