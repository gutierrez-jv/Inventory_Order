using Inventory_Order.Models.Database;
using Inventory_Order.ViewModels.Order;

namespace Inventory_Order.Service.Order
{
    public interface IOrderServ
    {
        Task<List<OrderTb>> GetAllOrdersAsync();
        Task<OrderTb?> GetOrderByIdAsync(int id);
        Task<List<OrderTb>> GetOrdersByCustomerIdAsync(int customerId);
        Task<bool> CreateOrderAsync(CreateOrderRequestViewModel request);
        Task<bool> CompleteOrderAsync(int orderId);
        Task<bool> CancelOrderAsync(int orderId);
    }
}