using Inventory_Order.Models.Database;

namespace Inventory_Order.Repository.OrderRepository
{
    public interface IOrderRepo
    {
            Task<List<OrderTb>> GetAllOrders();
            Task<OrderTb?> GetOrderById(int id);
            void AddOrder(OrderTb order);
            void UpdateOrder(OrderTb order);
            void DeleteOrder(int id);
    }
}
