using Inventory_Order.Models.Database;
using Inventory_Order.ViewModels.Order;

namespace Inventory_Order.Service.Order
{
    // Interface defining the contract for order-related business logic operations
    public interface IOrderServ
    {
        Task<List<OrderTb>> GetAllOrdersAsync(); // Retrieves a list of all orders
        Task<OrderTb?> GetOrderByIdAsync(int id); // Retrieves a specific order by its ID, returning null if not found
        Task<List<OrderTb>> GetOrdersByCustomerIdAsync(int customerId); // Retrieves a list of orders associated with a specific customer ID
        Task<bool> CreateOrderAsync(CreateOrderRequestViewModel request); // Creates a new order based on the provided request data, returning true if successful
        Task<bool> CompleteOrderAsync(int orderId); // Marks an existing order as completed based on its ID, returning true if successful
        Task<bool> CancelOrderAsync(int orderId); // Cancels an existing order based on its ID, returning true if successful
        Task<bool> UpdateOrderAsync(UpdateOrderRequestViewModel request); // Updates an existing order based on the provided request data, returning true if successful
        Task<bool> DeleteOrderAsync(int orderId); // Deletes an existing order based on its ID, returning true if successful
    }
}