using Inventory_Order.Models.Database; 
using Inventory_Order.Repository.CustomerRepository; 
using Inventory_Order.Repository.OrderRepository; 
using Inventory_Order.ViewModels.Order; 
using Microsoft.EntityFrameworkCore; 

namespace Inventory_Order.Service.Order
{
    // Handles business logic for orders (validation, stock control, calculations)
    public class OrderServ : IOrderServ
    {
        // Allowed order statuses
        private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Pending",
            "Completed",
            "Cancelled"
        };

        private readonly IOrderRepo _orderRepo; // Handles database operations for orders
        private readonly ICustomerRepo _customerRepo; // Used to validate customers
        private readonly InventoryOrderDbContext _dbContext; // Used for transactions and direct DB access

        public OrderServ(
            IOrderRepo orderRepo,
            ICustomerRepo customerRepo,
            InventoryOrderDbContext dbContext)
        {
            _orderRepo = orderRepo;
            _customerRepo = customerRepo;
            _dbContext = dbContext;
        }

        public async Task<List<OrderTb>> GetAllOrdersAsync()
        {
            return await _orderRepo.GetAllOrdersWithDetailsAsync();
        }

        // Retrieves a specific order by ID with validation
        public async Task<OrderTb?> GetOrderByIdAsync(int id)
        {
            if (id <= 0) // Reject invalid ID
                return null;

            return await _orderRepo.GetOrderByIdWithDetailsAsync(id);
        }

        // Retrieves all orders for a specific customer
        public async Task<List<OrderTb>> GetOrdersByCustomerIdAsync(int customerId)
        {
            if (customerId <= 0) // Reject invalid ID
                return new List<OrderTb>();

            var orders = await _orderRepo.GetAllOrdersWithDetailsAsync();
            return orders.Where(o => o.CustomersId == customerId).ToList(); // Filter orders by customer
        }

        // Creates a new order with validation and stock handling
        public async Task<bool> CreateOrderAsync(CreateOrderRequestViewModel request)
        {
            // Validate request
            if (request == null || request.Items == null || !request.Items.Any())
                return false;

            // Validate customer exists and is active
            var customer = await _customerRepo.GetCustomerByIdAsync(request.CustomersId);
            if (customer == null || !customer.IsActive)
                return false;

            // Start transaction (all operations must succeed together)
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Group duplicate products and sum quantities
                var groupedItems = request.Items
                    .GroupBy(i => i.ProductsId)
                    .Select(g => new
                    {
                        ProductsId = g.Key,
                        Quantity = g.Sum(x => x.Quantity)
                    })
                    .ToList();

                decimal totalAmount = 0; // Total order amount
                var orderItems = new List<OrderItemTb>(); // List of order items

                foreach (var item in groupedItems)
                {
                    if (item.Quantity <= 0)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }

                    // Check product exists and has enough stock
                    var product = await _dbContext.ProductTbs.FirstOrDefaultAsync(p => p.ProductsId == item.ProductsId);
                    if (product == null || product.Quantity < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }

                    // Calculate total for this item
                    var lineTotal = product.Price * item.Quantity;
                    totalAmount += lineTotal;

                    // Create order item
                    orderItems.Add(new OrderItemTb
                    {
                        ProductsId = product.ProductsId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        LineTotal = lineTotal
                    });

                    // Reduce product stock
                    product.Quantity -= item.Quantity;
                }

                // Create main order
                var order = new OrderTb
                {
                    CustomersId = request.CustomersId,
                    TotalAmount = totalAmount,
                    OrderStatus = "Pending",
                    DateCreated = DateTime.Now
                };

                await _dbContext.OrderTbs.AddAsync(order);
                await _dbContext.SaveChangesAsync();

                // Assign OrderId to each item
                foreach (var item in orderItems)
                {
                    item.OrdersId = order.OrdersId;
                }

                // Save order items
                await _dbContext.OrderItemTbs.AddRangeAsync(orderItems);
                await _dbContext.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                // Rollback if anything fails
                await transaction.RollbackAsync();
                return false;
            }
        }

        // Marks an order as completed
        public async Task<bool> CompleteOrderAsync(int orderId)
        {
            if (orderId <= 0)
                return false;

            var order = await _orderRepo.GetOrderByIdWithDetailsAsync(orderId);
            if (order == null)
                return false;

            order.OrderStatus = "Completed"; // Set status
            await _orderRepo.UpdateOrderAsync(order);
            return true;
        }

        // Cancels an order and restores stock
        public async Task<bool> CancelOrderAsync(int orderId)
        {
            if (orderId <= 0)
                return false;

            // Starts a database transaction, which groups multiple database actions into one safe unit of work.
            // This is used so all related changes are saved together, or none of them are saved if something fails.
            // Atomicity means "all or nothing" — either every step succeeds, or everything is undone to keep the data consistent.
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Load order with items to restore stock
                var order = await _dbContext.OrderTbs
                    .Include(o => o.OrderItemTbs)
                    .FirstOrDefaultAsync(o => o.OrdersId == orderId);

                if (order == null)
                    return false;

                // Restore stock if not already cancelled
                if (!string.Equals(order.OrderStatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var item in order.OrderItemTbs)
                    {
                        // For each item in the order, find the corresponding product and add back the quantity to the stock.
                        var product = await _dbContext.ProductTbs.FirstOrDefaultAsync(p => p.ProductsId == item.ProductsId);
                        if (product != null)
                            product.Quantity += item.Quantity;
                    }
                }

                order.OrderStatus = "Cancelled";
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync(); // If any error occurs, undo all changes to maintain data integrity
                return false;
            }
        }

        // Updates an order with full validation and stock recalculation
        public async Task<bool> UpdateOrderAsync(UpdateOrderRequestViewModel request)
        {
            // Validate input
            if (request == null || request.OrdersId <= 0 || request.CustomersId <= 0)
                return false;

            if (request.Items == null || request.Items.Count == 0)
                return false;

            if (!AllowedStatuses.Contains(request.OrderStatus)) // Validate order status
                return false;

            var customer = await _customerRepo.GetCustomerByIdAsync(request.CustomersId);
            if (customer == null || !customer.IsActive)
                return false;

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var order = await _dbContext.OrderTbs
                    .Include(o => o.OrderItemTbs)
                    .FirstOrDefaultAsync(o => o.OrdersId == request.OrdersId);

                if (order == null)
                    return false;

                // Restore stock from old items
                foreach (var oldItem in order.OrderItemTbs)
                {
                    var oldProduct = await _dbContext.ProductTbs.FirstOrDefaultAsync(p => p.ProductsId == oldItem.ProductsId);
                    if (oldProduct != null)
                        oldProduct.Quantity += oldItem.Quantity;
                }

                // Remove old items
                _dbContext.OrderItemTbs.RemoveRange(order.OrderItemTbs);

                // Process new items
                var groupedItems = request.Items
                    .GroupBy(i => i.ProductsId)
                    .Select(g => new
                    {
                        ProductsId = g.Key,
                        Quantity = g.Sum(x => x.Quantity)
                    })
                    .ToList();

                decimal total = 0;
                var newItems = new List<OrderItemTb>(); // List to hold new order items for saving

                foreach (var item in groupedItems) // Validate each item and calculate totals
                {
                    if (item.Quantity <= 0)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }

                    // Check product exists and has enough stock
                    var product = await _dbContext.ProductTbs.FirstOrDefaultAsync(p => p.ProductsId == item.ProductsId);
                    if (product == null || product.Quantity < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }

                    // Reduce stock for new items
                    product.Quantity -= item.Quantity;

                    // Calculate line total for this item and add to order total
                    var lineTotal = product.Price * item.Quantity;
                    total += lineTotal;

                    // Create new order item and add to list for saving
                    newItems.Add(new OrderItemTb
                    {
                        OrdersId = order.OrdersId,
                        ProductsId = product.ProductsId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        LineTotal = lineTotal
                    });
                }

                // Update order details
                order.CustomersId = request.CustomersId;
                // Set the order status to the validated value from the allowed statuses
                order.OrderStatus = AllowedStatuses.First(s => s.Equals(request.OrderStatus, StringComparison.OrdinalIgnoreCase)); 
                order.TotalAmount = total;

                await _dbContext.OrderItemTbs.AddRangeAsync(newItems);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        // Deletes an order and restores stock
        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            if (orderId <= 0)
                return false;

            using var transaction = await _dbContext.Database.BeginTransactionAsync(); // Start transaction to ensure all operations succeed together

            try
            {
                // Load order with items to restore stock before deletion
                var order = await _dbContext.OrderTbs
                    .Include(o => o.OrderItemTbs)
                    .FirstOrDefaultAsync(o => o.OrdersId == orderId);

                if (order == null)
                    return false;

                // Restore stock before deleting
                foreach (var item in order.OrderItemTbs)
                {
                    var product = await _dbContext.ProductTbs.FirstOrDefaultAsync(p => p.ProductsId == item.ProductsId);
                    if (product != null)
                        product.Quantity += item.Quantity;
                }

                _dbContext.OrderItemTbs.RemoveRange(order.OrderItemTbs);
                _dbContext.OrderTbs.Remove(order);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync(); // Commit transaction to finalize changes
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}