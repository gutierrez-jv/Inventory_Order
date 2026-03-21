using Inventory_Order.Models.Database;

namespace Inventory_Order.Repository.OrderRepository
{
    public interface IOrderRepo
    {
        Task<List<OrderTb>> GetAllOrdersWithDetailsAsync();
        Task<OrderTb?> GetOrderByIdWithDetailsAsync(int id);

        Task<OrderTb> AddOrderAsync(OrderTb order);
        Task AddOrderItemsAsync(List<OrderItemTb> orderItems);

        Task UpdateOrderAsync(OrderTb order);
        Task DeleteOrderAsync(int id);
    }
}