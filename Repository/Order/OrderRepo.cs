using Inventory_Order.Models.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Order.Repository.OrderRepository
{
    public class OrderRepo : IOrderRepo
    {
        private readonly InventoryOrderDbContext _context;
        public OrderRepo(InventoryOrderDbContext context)
        {
            _context = context;
        }
        public async Task<OrderTb?> AddOrder(OrderTb order)
        {
            _context.OrderTbs.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public void DeleteOrder(int id)
        {
            var ex = _context.OrderTbs.Find(id);
            if (ex != null) _context.OrderTbs.Remove(ex);
            _context.SaveChanges();
        }

        public async Task<List<OrderTb>> GetAllOrders()
        {
            return await _context.OrderTbs.ToListAsync();
        }

        public async Task<OrderTb?> GetOrderById(int id)
        {
            return await _context.OrderTbs.FindAsync(id);
        }

        public void UpdateOrder(OrderTb order)
        {
            _context.OrderTbs.Update(order);
            _context.SaveChanges();
        }
    }
}
