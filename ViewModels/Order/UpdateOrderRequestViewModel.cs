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
        [Range(0.01, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string OrderStatus { get; set; } = "Pending";

        [Required]
        public DateTime DateCreated { get; set; }
    }
}
