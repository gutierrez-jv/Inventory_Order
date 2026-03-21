using Inventory_Order.Models.Database;
using Inventory_Order.Repository.CustomerRepository;
using Inventory_Order.Repository.OrderRepository;
using Inventory_Order.Repository.ProductRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace Inventory_Order.Service.Order
{
    public class OrderServ : IOrderServ
    {
        private readonly IOrderRepo _orderRepo;
        private readonly IProductRepo _productRepo;
        private readonly ICustomerRepo _customerRepo;

        public OrderServ(IOrderRepo orderRepo, IProductRepo productRepo, ICustomerRepo customerRepo)
        {
            _orderRepo = orderRepo;
            _customerRepo = customerRepo;
            _productRepo = productRepo;
        }

        public async Task<bool?> CreateOrder(OrderTb order)
        {
            if (order == null) return false;
            if (order.Quantity <= 0) return false;

            var customer = await _customerRepo.GetCustomerById(order.CustomersId);
            if (customer == null) return false;
            if (!customer.IsActive) return false;

            var product = await _productRepo.GetProductById(order.ProductsId);
            if (product == null) return false;
            if (product.Quantity <= 0) return false;
            if (order.Quantity > product.Quantity) return false;

            order.Amount = order.Quantity * product.Price;

            product.Quantity -= order.Quantity;
            product.Stock = product.Quantity > 0;

            await _orderRepo.AddOrder(order);
            await _productRepo.UpdateProduct(product);
            return false;
        }

        public bool DeleteOrder(int id)
        {
            if (id <= 0) return false;

            var existingOrder = _orderRepo.GetOrderById(id);
            if (existingOrder == null) return false;

            _orderRepo.DeleteOrder(id);
            return true;
        }

        public IEnumerable<OrderTb> GetAllOrders()
        {
            return _orderRepo.GetAllOrders().Result ?? Enumerable.Empty<OrderTb>();
        }

        public OrderTb? GetOrderById(int id)
        {
            if (id <= 0) return null;
            return _orderRepo.GetOrderById(id).Result;
        }

        public bool UpdateOrder(OrderTb order)
        {
            if (order == null) return false;
            if (order.OrdersId <= 0) return false;
            if (order.Quantity <= 0) return false;

            var existing = _orderRepo.GetOrderById(order.OrdersId).Result;
            if (existing == null) return false;

            var customer = _customerRepo.GetCustomerById(order.CustomersId).Result;
            if (customer == null) return false;
            if (!customer.IsActive) return false;

            var product = _productRepo.GetProductById(order.ProductsId).Result;
            if (product == null) return false;

            order.Amount = order.Quantity * product.Price;

            _orderRepo.UpdateOrder(order);
            return true;
        }
    }   
}
