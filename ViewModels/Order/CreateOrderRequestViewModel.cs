using System.ComponentModel.DataAnnotations;

namespace Inventory_Order.ViewModels.Order
{
    public class CreateOrderRequestViewModel
    {
        [Required]
        public int CustomersId { get; set; }

        [Required]
        public List<OrderItemRequestViewModel> Items { get; set; } = new();
    }
}