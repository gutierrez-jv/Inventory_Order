using System.ComponentModel.DataAnnotations;

namespace Inventory_Order.ViewModels.Order
{
    public class UpdateOrderRequestViewModel
    {
        [Required]
        [Range(1, int.MaxValue)] // Ensures a valid order ID is provided
        public int OrdersId { get; set; }

        [Required]
        [Range(1, int.MaxValue)] // Ensures a valid customer ID is provided
        public int CustomersId { get; set; }

        [Required]
        [StringLength(20)]
        public string OrderStatus { get; set; } = "Pending";

        [Required]
        [MinLength(1, ErrorMessage = "At least one order item is required.")] // Ensures that the order contains at least one item
        public List<OrderItemRequestViewModel> Items { get; set; } = new(); // List of items to update in the order
    }
}
