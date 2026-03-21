using Inventory_Order.Models.Database;
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

        public async Task<List<OrderTb>> GetAllOrdersWithDetailsAsync()
        {
            return await _context.OrderTbs
                .Include(o => o.Customers)
                .Include(o => o.OrderItemTbs)
                    .ThenInclude(oi => oi.Products)
                .ToListAsync();
        }

        public async Task<OrderTb?> GetOrderByIdWithDetailsAsync(int id)
        {
            return await _context.OrderTbs
                .Include(o => o.Customers)
                .Include(o => o.OrderItemTbs)
                    .ThenInclude(oi => oi.Products)
                .FirstOrDefaultAsync(o => o.OrdersId == id);
        }

        public async Task<OrderTb> AddOrderAsync(OrderTb order)
        {
            await _context.OrderTbs.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task AddOrderItemsAsync(List<OrderItemTb> orderItems)
        {
            await _context.OrderItemTbs.AddRangeAsync(orderItems);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrderAsync(OrderTb order)
        {
            _context.OrderTbs.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOrderAsync(int id)
        {
            var existingOrder = await _context.OrderTbs
                .Include(o => o.OrderItemTbs)
                .FirstOrDefaultAsync(o => o.OrdersId == id);

            if (existingOrder != null)
            {
                if (existingOrder.OrderItemTbs != null && existingOrder.OrderItemTbs.Any())
                {
                    _context.OrderItemTbs.RemoveRange(existingOrder.OrderItemTbs);
                }

                _context.OrderTbs.Remove(existingOrder);
                await _context.SaveChangesAsync();
            }
        }
    }
}