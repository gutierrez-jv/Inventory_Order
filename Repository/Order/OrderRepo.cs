using Inventory_Order.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Order.Repository.OrderRepository
{
    // Handles direct database operations for orders and order items
    public class OrderRepo : IOrderRepo
    {
        // Database context used to access order data
        private readonly InventoryOrderDbContext _context;

        public OrderRepo(InventoryOrderDbContext context)
        {
            _context = context;
        }

        // Retrieves all orders together with customer and product details
        public async Task<List<OrderTb>> GetAllOrdersWithDetailsAsync()
        {
            return await _context.OrderTbs
                .Include(o => o.Customers) // Load related customer
                .Include(o => o.OrderItemTbs) // Load related order items
                    .ThenInclude(oi => oi.Products) // Load related products for each order item
                .ToListAsync();
        }

        // Retrieves a specific order together with customer and product details
        public async Task<OrderTb?> GetOrderByIdWithDetailsAsync(int id)
        {
            return await _context.OrderTbs
                .Include(o => o.Customers) 
                .Include(o => o.OrderItemTbs) 
                    .ThenInclude(oi => oi.Products) 
                .FirstOrDefaultAsync(o => o.OrdersId == id);
        }

        // Adds a new order to the database and returns the saved order
        public async Task<OrderTb> AddOrderAsync(OrderTb order)
        {
            await _context.OrderTbs.AddAsync(order); // Add the order
            await _context.SaveChangesAsync();
            return order;
        }

        // Adds all order items related to an order
        public async Task AddOrderItemsAsync(List<OrderItemTb> orderItems)
        {
            await _context.OrderItemTbs.AddRangeAsync(orderItems);
            await _context.SaveChangesAsync();
        }

        // Updates an existing order record
        public async Task UpdateOrderAsync(OrderTb order)
        {
            _context.OrderTbs.Update(order);
            await _context.SaveChangesAsync();
        }

        // Deletes an order and removes related order items first
        public async Task DeleteOrderAsync(int id)
        {
            // Find the existing order with related items
            var existingOrder = await _context.OrderTbs
                .Include(o => o.OrderItemTbs)
                .FirstOrDefaultAsync(o => o.OrdersId == id);

            if (existingOrder != null)
            {
                // Remove related order items first
                if (existingOrder.OrderItemTbs != null && existingOrder.OrderItemTbs.Any())
                {
                    _context.OrderItemTbs.RemoveRange(existingOrder.OrderItemTbs);
                }

                // Remove the order itself
                _context.OrderTbs.Remove(existingOrder);
                await _context.SaveChangesAsync();
            }
        }
    }
}