using System.ComponentModel.DataAnnotations;

namespace Inventory_Order.ViewModels.Order
{
    public class CreateOrderRequestViewModel
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Customer is required.")] // Ensures a valid customer ID is provided
        public int CustomersId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one order item is required.")] // Ensures that the order contains at least one item
        public List<OrderItemRequestViewModel> Items { get; set; } = new();
    }
}
