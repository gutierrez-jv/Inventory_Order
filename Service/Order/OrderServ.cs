using Inventory_Order.Models.Database;
using Inventory_Order.Repository.CustomerRepository;
using Inventory_Order.Repository.OrderRepository;
using Inventory_Order.Repository.ProductRepository;
using Inventory_Order.ViewModels.Order;

namespace Inventory_Order.Service.Order
{
    public class OrderServ : IOrderServ
    {
        private readonly IOrderRepo _orderRepo;
        private readonly IProductRepo _productRepo;
        private readonly ICustomerRepo _customerRepo;

        public OrderServ(
            IOrderRepo orderRepo,
            IProductRepo productRepo,
            ICustomerRepo customerRepo)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _customerRepo = customerRepo;
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
            if (request == null)
                return false;

            if (request.Items == null || !request.Items.Any())
                return false;

            var customer = await _customerRepo.GetCustomerByIdAsync(request.CustomersId);

            // Rule 1: Customer must exist
            if (customer == null)
                return false;

            // Rule 2: Customer must be active before ordering
            if (!customer.IsActive)
                return false;

            decimal totalAmount = 0;
            List<OrderItemTb> orderItems = new();

            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0)
                    return false;

                var product = await _productRepo.GetProductByIdAsync(item.ProductsId);

                if (product == null)
                    return false;

                // Rule 5: Cannot order if stock is 0
                if (product.Quantity <= 0)
                    return false;

                if (item.Quantity > product.Quantity)
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

                // Rule 4: Reduce stock after successful order
                product.Quantity -= item.Quantity;
                await _productRepo.UpdateProductAsync(product);
            }

            // Rule 3: Total amount calculated in service
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
            if (request == null)
                return false;

            if (request.OrdersId <= 0 || request.CustomersId <= 0 || request.TotalAmount <= 0)
                return false;

            var validStatuses = new[] { "Pending", "Completed", "Cancelled", "Processing" };
            if (!validStatuses.Contains(request.OrderStatus))
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
    }
}