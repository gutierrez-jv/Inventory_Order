namespace Inventory_Order.ViewModels.Order
{
    public class OrderIndexItemViewModel
    {
        public int OrdersId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int Amount { get; set; }
        public string OrderStatus { get; set; } = "Pending";
        public string DateCreated { get; set; } = "N/A";
    }
}
