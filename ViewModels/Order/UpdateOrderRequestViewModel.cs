using System.ComponentModel.DataAnnotations;

namespace Inventory_Order.ViewModels.Order
{
    public class UpdateOrderRequestViewModel
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int OrdersId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CustomersId { get; set; }

        [Required]
        [StringLength(20)]
        public string OrderStatus { get; set; } = "Pending";

        [Required]
        [MinLength(1, ErrorMessage = "At least one order item is required.")]
        public List<OrderItemRequestViewModel> Items { get; set; } = new();
    }
}
