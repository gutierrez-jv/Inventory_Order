using Inventory_Order.Models.Database;

namespace Inventory_Order.Repository.OrderRepository
{
    // This interface defines the contract for the Order repository,
    // which includes methods for retrieving, adding, updating, and deleting orders along with their related details.
    public interface IOrderRepo
    {
        Task<List<OrderTb>> GetAllOrdersWithDetailsAsync(); // Retrieves all orders along with their related details (e.g., customer information, order items).
        Task<OrderTb?> GetOrderByIdWithDetailsAsync(int id); // Retrieves a specific order by its ID along with its related details, returns null if not found
        Task<OrderTb> AddOrderAsync(OrderTb order); // Adds a new order to the database and returns the added order, which may include generated fields like the order ID.
        Task AddOrderItemsAsync(List<OrderItemTb> orderItems); // Adds a list of order items to the database, which are associated with an order. 
        Task UpdateOrderAsync(OrderTb order); // Updates an existing order in the database.
        Task DeleteOrderAsync(int id); // Deletes an order from the database by its ID
    }
}