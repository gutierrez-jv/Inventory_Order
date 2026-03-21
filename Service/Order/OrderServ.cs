using Inventory_Order.Models.Database;
using Inventory_Order.Repository.CustomerRepository;
using Inventory_Order.Repository.OrderRepository;
using Inventory_Order.ViewModels.Order;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Order.Service.Order
{
    public class OrderServ : IOrderServ
    {
        private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Pending",
            "Completed",
            "Cancelled"
        };

        private readonly IOrderRepo _orderRepo;
        private readonly ICustomerRepo _customerRepo;
        private readonly InventoryOrderDbContext _dbContext;

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

        public async Task<OrderTb?> GetOrderByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            return await _orderRepo.GetOrderByIdWithDetailsAsync(id);
        }

        public async Task<List<OrderTb>> GetOrdersByCustomerIdAsync(int customerId)
        {
            if (customerId <= 0)
                return new List<OrderTb>();

            var orders = await _orderRepo.GetAllOrdersWithDetailsAsync();
            return orders.Where(o => o.CustomersId == customerId).ToList();
        }

        public async Task<bool> CreateOrderAsync(CreateOrderRequestViewModel request)
        {
            if (request == null || request.Items == null || !request.Items.Any())
                return false;

            var customer = await _customerRepo.GetCustomerByIdAsync(request.CustomersId);
            if (customer == null || !customer.IsActive)
                return false;

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var groupedItems = request.Items
                    .GroupBy(i => i.ProductsId)
                    .Select(g => new
                    {
                        ProductsId = g.Key,
                        Quantity = g.Sum(x => x.Quantity)
                    })
                    .ToList();

                decimal totalAmount = 0;
                var orderItems = new List<OrderItemTb>();

                foreach (var item in groupedItems)
                {
                    if (item.Quantity <= 0)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }

                    var product = await _dbContext.ProductTbs.FirstOrDefaultAsync(p => p.ProductsId == item.ProductsId);
                    if (product == null || product.Quantity < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }

                    var lineTotal = product.Price * item.Quantity;
                    totalAmount += lineTotal;

                    orderItems.Add(new OrderItemTb
                    {
                        ProductsId = product.ProductsId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        LineTotal = lineTotal
                    });

                    product.Quantity -= item.Quantity;
                }

                var order = new OrderTb
                {
                    CustomersId = request.CustomersId,
                    TotalAmount = totalAmount,
                    OrderStatus = "Pending",
                    DateCreated = DateTime.Now
                };

                await _dbContext.OrderTbs.AddAsync(order);
                await _dbContext.SaveChangesAsync();

                foreach (var item in orderItems)
                {
                    item.OrdersId = order.OrdersId;
                }

                await _dbContext.OrderItemTbs.AddRangeAsync(orderItems);
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

        public async Task<bool> CompleteOrderAsync(int orderId)
        {
            if (orderId <= 0)
                return false;

            var order = await _orderRepo.GetOrderByIdWithDetailsAsync(orderId);
            if (order == null)
                return false;

            order.OrderStatus = "Completed";
            await _orderRepo.UpdateOrderAsync(order);
            return true;
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            if (orderId <= 0)
                return false;

            var order = await _orderRepo.GetOrderByIdWithDetailsAsync(orderId);
            if (order == null)
                return false;

            order.OrderStatus = "Cancelled";
            await _orderRepo.UpdateOrderAsync(order);
            return true;
        }

        public async Task<bool> UpdateOrderAsync(UpdateOrderRequestViewModel request)
        {
            if (request == null || request.OrdersId <= 0 || request.CustomersId <= 0)
                return false;

            if (request.Items == null || request.Items.Count == 0)
                return false;

            if (!AllowedStatuses.Contains(request.OrderStatus))
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

                foreach (var oldItem in order.OrderItemTbs)
                {
                    var oldProduct = await _dbContext.ProductTbs.FirstOrDefaultAsync(p => p.ProductsId == oldItem.ProductsId);
                    if (oldProduct != null)
                        oldProduct.Quantity += oldItem.Quantity;
                }

                _dbContext.OrderItemTbs.RemoveRange(order.OrderItemTbs);

                var groupedItems = request.Items
                    .GroupBy(i => i.ProductsId)
                    .Select(g => new
                    {
                        ProductsId = g.Key,
                        Quantity = g.Sum(x => x.Quantity)
                    })
                    .ToList();

                decimal total = 0;
                var newItems = new List<OrderItemTb>();

                foreach (var item in groupedItems)
                {
                    if (item.Quantity <= 0)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }

                    var product = await _dbContext.ProductTbs.FirstOrDefaultAsync(p => p.ProductsId == item.ProductsId);
                    if (product == null || product.Quantity < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }

                    product.Quantity -= item.Quantity;

                    var lineTotal = product.Price * item.Quantity;
                    total += lineTotal;

                    newItems.Add(new OrderItemTb
                    {
                        OrdersId = order.OrdersId,
                        ProductsId = product.ProductsId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        LineTotal = lineTotal
                    });
                }

                order.CustomersId = request.CustomersId;
                order.OrderStatus = AllowedStatuses.First(s => s.Equals(request.OrderStatus, StringComparison.OrdinalIgnoreCase));
                order.TotalAmount = total;
                order.DateCreated = DateTime.Now;

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

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            if (orderId <= 0)
                return false;

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var order = await _dbContext.OrderTbs
                    .Include(o => o.OrderItemTbs)
                    .FirstOrDefaultAsync(o => o.OrdersId == orderId);

                if (order == null)
                    return false;

                foreach (var item in order.OrderItemTbs)
                {
                    var product = await _dbContext.ProductTbs.FirstOrDefaultAsync(p => p.ProductsId == item.ProductsId);
                    if (product != null)
                        product.Quantity += item.Quantity;
                }

                _dbContext.OrderItemTbs.RemoveRange(order.OrderItemTbs);
                _dbContext.OrderTbs.Remove(order);
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
    }
}
