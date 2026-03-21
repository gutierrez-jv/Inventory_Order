using Inventory_Order.Models.Database;
using Inventory_Order.Repository.CustomerRepository;
using Inventory_Order.Repository.OrderRepository;
using Inventory_Order.Repository.ProductRepository;
using Inventory_Order.ViewModels.Order;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Order.Service.Order
{
    public class OrderServ : IOrderServ
    {
        private readonly IOrderRepo _orderRepo;
        private readonly IProductRepo _productRepo;
        private readonly ICustomerRepo _customerRepo;
        private readonly InventoryOrderDbContext _dbContext;

        public OrderServ(
            IOrderRepo orderRepo,
            IProductRepo productRepo,
            ICustomerRepo customerRepo,
            InventoryOrderDbContext dbContext)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;
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

            decimal totalAmount = 0;
            List<OrderItemTb> orderItems = new();

            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0)
                    return false;

                var product = await _productRepo.GetProductByIdAsync(item.ProductsId);
                if (product == null || product.Quantity <= 0 || item.Quantity > product.Quantity)
                    return false;

                decimal lineTotal = product.Price * item.Quantity;
                totalAmount += lineTotal;

                orderItems.Add(new OrderItemTb
                {
                    ProductsId = product.ProductsId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    LineTotal = lineTotal
                });

                product.Quantity -= item.Quantity;
                await _productRepo.UpdateProductAsync(product);
            }

            var order = new OrderTb
            {
                CustomersId = request.CustomersId,
                TotalAmount = totalAmount,
                OrderStatus = "Pending",
                DateCreated = DateTime.Now
            };

            order = await _orderRepo.AddOrderAsync(order);

            foreach (var item in orderItems)
            {
                item.OrdersId = order.OrdersId;
            }

            await _orderRepo.AddOrderItemsAsync(orderItems);
            return true;
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

            if (request.OrderStatus != "Pending" &&
                request.OrderStatus != "Processing" &&
                request.OrderStatus != "Completed" &&
                request.OrderStatus != "Cancelled")
                return false;

            var customer = await _customerRepo.GetCustomerByIdAsync(request.CustomersId);
            if (customer == null || !customer.IsActive)
                return false;

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var order = await _dbContext.OrderTbs
                .Include(o => o.OrderItemTbs)
                .FirstOrDefaultAsync(o => o.OrdersId == request.OrdersId);

            if (order == null)
                return false;

            // Return previous stock from old items
            foreach (var oldItem in order.OrderItemTbs)
            {
                var oldProduct = await _dbContext.ProductTbs.FirstOrDefaultAsync(p => p.ProductsId == oldItem.ProductsId);
                if (oldProduct != null)
                    oldProduct.Quantity += oldItem.Quantity;
            }

            _dbContext.OrderItemTbs.RemoveRange(order.OrderItemTbs);

            decimal total = 0;
            var newItems = new List<OrderItemTb>();

            foreach (var item in request.Items)
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
            order.OrderStatus = request.OrderStatus;
            order.TotalAmount = total;
            order.DateCreated = DateTime.Now; // use as last edited timestamp

            await _dbContext.OrderItemTbs.AddRangeAsync(newItems);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            if (orderId <= 0)
                return false;

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

            await _orderRepo.DeleteOrderAsync(orderId);
            return true;
        }

        public async Task<bool> UpdateOrderAsync(UpdateOrderRequestViewModel request)
        {
            if (request == null)
                return false;

            if (request.OrdersId <= 0 || request.CustomersId <= 0 || request.TotalAmount <= 0)
                return false;

            if (request.OrderStatus != "Pending" &&
                request.OrderStatus != "Processing" &&
                request.OrderStatus != "Completed" &&
                request.OrderStatus != "Cancelled")
                return false;

            var existingOrder = await _orderRepo.GetOrderByIdWithDetailsAsync(request.OrdersId);
            if (existingOrder == null)
                return false;

            var customer = await _customerRepo.GetCustomerByIdAsync(request.CustomersId);
            if (customer == null || !customer.IsActive)
                return false;

            existingOrder.CustomersId = request.CustomersId;
            existingOrder.TotalAmount = request.TotalAmount;
            existingOrder.OrderStatus = request.OrderStatus;
            existingOrder.DateCreated = request.DateCreated;

            await _orderRepo.UpdateOrderAsync(existingOrder);
            return true;
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            if (orderId <= 0)
                return false;

            var order = await _orderRepo.GetOrderByIdWithDetailsAsync(orderId);
            if (order == null)
                return false;

            await _orderRepo.DeleteOrderAsync(orderId);
            return true;
        }
    }
}
