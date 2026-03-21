using Inventory_Order.Service.Customer;
using Inventory_Order.Service.Order;
using Inventory_Order.Service.Product;
using Inventory_Order.ViewModels.Order;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inventory_Order.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderServ _orderService;
        private readonly ICustomerServ _customerService;
        private readonly IProductServ _productService;

        public OrderController(IOrderServ orderService, ICustomerServ customerService, IProductServ productService)
        {
            _orderService = orderService;
            _customerService = customerService;
            _productService = productService;
        }

        public IActionResult Index()
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Account");

            var orders = _orderService.GetAllOrders();
            var customers = _customerService.GetAllCustomers().ToDictionary(c => c.CustomerId, c => $"{c.FirstName} {c.LastName}");
            var products = _productService.GetAllProducts().ToDictionary(p => p.ProductsId, p => p.Name);

            var model = orders.Select(o => new OrderIndexItemViewModel
            {
                OrdersId = o.OrdersId,
                CustomerName = customers.TryGetValue(o.CustomersId, out var customerName) ? customerName : "Unknown",
                ProductName = products.TryGetValue(o.ProductsId, out var productName) ? productName : "Unknown",
                Quantity = o.Quantity,
                Amount = o.Amount,
                OrderStatus = "Pending",
                DateCreated = "N/A"
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Account");

            var order = _orderService.GetOrderById(id);
            if (order == null) return NotFound();

            var customer = _customerService.GetCustomerById(order.CustomersId).Result;
            var product = _productService.GetProductById(order.ProductsId).Result;

            var model = new OrderDetailsViewModel
            {
                OrdersId = order.OrdersId,
                CustomersId = order.CustomersId,
                CustomerName = customer == null ? "Unknown" : $"{customer.FirstName} {customer.LastName}",
                ProductsId = order.ProductsId,
                ProductName = product?.Name ?? "Unknown",
                Quantity = order.Quantity,
                Amount = order.Amount,
                OrderStatus = "Pending",
                DateCreated = "N/A"
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Account");

            PopulateDropdowns();
            return View(new OrderFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(OrderFormViewModel model)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                PopulateDropdowns();
                return View(model);
            }

            var order = new OrderTb
            {
                OrdersId = model.OrdersId,
                CustomersId = model.CustomersId,
                ProductsId = model.ProductsId,
                Quantity = model.Quantity
            };

            var success = _orderService.CreateOrder(order).Result == true;
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to create order. Check IDs, customer active status, and product stock.");
                PopulateDropdowns();
                return View(model);
            }

            TempData["SuccessMessage"] = "Order created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Account");

            var order = _orderService.GetOrderById(id);
            if (order == null) return NotFound();

            var model = new OrderFormViewModel
            {
                OrdersId = order.OrdersId,
                CustomersId = order.CustomersId,
                ProductsId = order.ProductsId,
                Quantity = order.Quantity
            };

            PopulateDropdowns();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(OrderFormViewModel model)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                PopulateDropdowns();
                return View(model);
            }

            var order = new OrderTb
            {
                OrdersId = model.OrdersId,
                CustomersId = model.CustomersId,
                ProductsId = model.ProductsId,
                Quantity = model.Quantity
            };

            var success = _orderService.UpdateOrder(order);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to update order. Validate customer/product references and quantity.");
                PopulateDropdowns();
                return View(model);
            }

            TempData["SuccessMessage"] = "Order updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Account");

            var success = _orderService.DeleteOrder(id);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? "Order cancelled successfully."
                : "Unable to cancel order.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkCompleted(int id)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Account");

            var order = _orderService.GetOrderById(id);
            TempData[order != null ? "SuccessMessage" : "ErrorMessage"] = order != null
                ? "Order marked as completed in UI. Note: current database table does not store order status."
                : "Order not found.";

            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropdowns()
        {
            ViewBag.Customers = new SelectList(
                _customerService.GetAllCustomers()
                    .Where(c => c.IsActive)
                    .Select(c => new
                    {
                        c.CustomerId,
                        FullName = $"{c.FirstName} {c.LastName}"
                    }),
                "CustomerId",
                "FullName");

            ViewBag.Products = new SelectList(
                _productService.GetAllProducts().Where(p => p.Stock),
                nameof(ProductTb.ProductsId),
                nameof(ProductTb.Name));
        }

        private bool IsAdminAuthenticated()
        {
            return HttpContext.Session.GetString("IsAdminLoggedIn") == "true";
        }
    }
}
