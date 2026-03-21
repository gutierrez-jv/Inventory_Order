using Inventory_Order.Models.Database;

namespace Inventory_Order.Service.Order
{
    public interface IOrderServ
    {
        IEnumerable<OrderTb> GetAllOrders();
        OrderTb? GetOrderById(int id);
        Task<bool?> CreateOrder(OrderTb order);
        bool UpdateOrder(OrderTb order);
        bool DeleteOrder(int id);
    }
}
