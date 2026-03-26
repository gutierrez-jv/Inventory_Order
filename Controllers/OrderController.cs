using Inventory_Order.Models.Database;
using Inventory_Order.Service.Customer;
using Inventory_Order.Service.Order;
using Inventory_Order.Service.Product;
using Inventory_Order.ViewModels.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inventory_Order.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderServ _orderServ;
        private readonly ICustomerServ _customerServ;
        private readonly IProductServ _productServ;

        public OrderController(
            IOrderServ orderServ,
            ICustomerServ customerServ,
            IProductServ productServ)
        {
            _orderServ = orderServ;
            _customerServ = customerServ;
            _productServ = productServ;
        }

        private int? GetCustomerId()
        {
            var value = User.FindFirst("CustomerId")?.Value;

            if (int.TryParse(value, out int customerId))
                return customerId;

            return null;
        }

        private async Task<OrderTb?> GetOwnOrderAsync(int id)
        {
            var customerId = GetCustomerId();
            if (!customerId.HasValue)
                return null;

            var order = await _orderServ.GetOrderByIdAsync(id);
            if (order == null || order.CustomersId != customerId.Value)
                return null;

            return order;
        }

        private async Task LoadOrderFormLookupsAsync(bool includeOutOfStockProducts = false)
        {
            var customers = await _customerServ.GetAllCustomersAsync();
            var products = await _productServ.GetAllProductsAsync();

            var customerItems = new List<SelectListItem>();
            foreach (var customer in customers)
            {
                if (!customer.IsActive)
                    continue;

                customerItems.Add(new SelectListItem
                {
                    Value = customer.CustomerId.ToString(),
                    Text = customer.FirstName + " " + customer.LastName + " (#" + customer.CustomerId + ")"
                });
            }

            var productItems = new List<SelectListItem>();
            foreach (var product in products)
            {
                if (!includeOutOfStockProducts && product.Quantity <= 0)
                    continue;

                productItems.Add(new SelectListItem
                {
                    Value = product.ProductsId.ToString(),
                    Text = product.Name + " (" + product.Barcode + ") - ₱" + product.Price.ToString("N2") + " | Stock: " + product.Quantity
                });
            }

            ViewBag.Customers = customerItems;
            ViewBag.Products = productItems;
        }

        private void SetSuccess(string message) => TempData["SuccessMessage"] = message;
        private void SetError(string message) => TempData["ErrorMessage"] = message;

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var orders = await _orderServ.GetAllOrdersAsync();
            return View(orders);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderServ.GetOrderByIdAsync(id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _orderServ.GetOrderByIdAsync(id);

            if (order == null)
                return NotFound();

            var model = new UpdateOrderRequestViewModel
            {
                OrdersId = order.OrdersId,
                CustomersId = order.CustomersId,
                OrderStatus = order.OrderStatus,
                Items = order.OrderItemTbs.Select(i => new OrderItemRequestViewModel
                {
                    ProductsId = i.ProductsId,
                    Quantity = i.Quantity
                }).ToList()
            };

            await LoadOrderFormLookupsAsync(includeOutOfStockProducts: true);
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateOrderRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadOrderFormLookupsAsync(includeOutOfStockProducts: true);
                return View(model);
            }

            bool success;
            try
            {
                success = await _orderServ.UpdateOrderAsync(model);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "A system error occurred while updating the order.");
                await LoadOrderFormLookupsAsync(includeOutOfStockProducts: true);
                return View(model);
            }

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to update order. Please verify customer, products, and stock levels.");
                await LoadOrderFormLookupsAsync(includeOutOfStockProducts: true);
                return View(model);
            }

            SetSuccess("Order updated successfully.");
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            bool success;
            try
            {
                success = await _orderServ.CompleteOrderAsync(id);
            }
            catch
            {
                SetError("A system error occurred while completing the order.");
                return RedirectToAction(nameof(Index));
            }

            if (!success)
                SetError("Unable to complete order. It may be invalid or missing.");
            else
                SetSuccess("Order marked as completed.");
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            bool success;
            try
            {
                success = await _orderServ.CancelOrderAsync(id);
            }
            catch
            {
                SetError("A system error occurred while cancelling the order.");
                return RedirectToAction(nameof(Index));
            }

            if (!success)
                SetError("Unable to cancel order. It may be invalid or missing.");
            else
                SetSuccess("Order cancelled successfully.");
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            bool success;
            try
            {
                success = await _orderServ.DeleteOrderAsync(id);
            }
            catch
            {
                SetError("A system error occurred while deleting the order.");
                return RedirectToAction(nameof(Index));
            }

            if (!success)
                SetError("Unable to delete order. It may be invalid or missing.");
            else
                SetSuccess("Order deleted successfully.");
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyOrders()
        {
            var customerId = GetCustomerId();

            if (!customerId.HasValue)
                return RedirectToAction("AccessDenied", "Account");

            var orders = await _orderServ.GetOrdersByCustomerIdAsync(customerId.Value);
            return View(orders);
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyOrderDetails(int id)
        {
            var order = await GetOwnOrderAsync(id);
            if (order == null)
                return RedirectToAction(nameof(MyOrders));

            return View(order);
        }

        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<IActionResult> MyOrderEdit(int id)
        {
            var order = await GetOwnOrderAsync(id);
            if (order == null)
                return RedirectToAction(nameof(MyOrders));

            var model = new UpdateOrderRequestViewModel
            {
                OrdersId = order.OrdersId,
                CustomersId = order.CustomersId,
                OrderStatus = order.OrderStatus,
                Items = order.OrderItemTbs.Select(i => new OrderItemRequestViewModel
                {
                    ProductsId = i.ProductsId,
                    Quantity = i.Quantity
                }).ToList()
            };

            await LoadOrderFormLookupsAsync(includeOutOfStockProducts: true);
            return View(model);
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyOrderEdit(UpdateOrderRequestViewModel model)
        {
            var order = await GetOwnOrderAsync(model.OrdersId);
            if (order == null)
                return RedirectToAction(nameof(MyOrders));

            model.CustomersId = order.CustomersId;
            ModelState.Remove(nameof(UpdateOrderRequestViewModel.CustomersId));

            if (!ModelState.IsValid)
            {
                await LoadOrderFormLookupsAsync(includeOutOfStockProducts: true);
                return View(model);
            }

            bool success;
            try
            {
                success = await _orderServ.UpdateOrderAsync(model);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "A system error occurred while updating your order.");
                await LoadOrderFormLookupsAsync(includeOutOfStockProducts: true);
                return View(model);
            }

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to update your order. Please verify products and stock levels.");
                await LoadOrderFormLookupsAsync(includeOutOfStockProducts: true);
                return View(model);
            }

            SetSuccess("Your order was updated successfully.");
            return RedirectToAction(nameof(MyOrders));
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyOrderDelete(int id)
        {
            var order = await GetOwnOrderAsync(id);
            if (order == null)
                return RedirectToAction(nameof(MyOrders));

            bool success;
            try
            {
                success = await _orderServ.DeleteOrderAsync(id);
            }
            catch
            {
                SetError("A system error occurred while deleting your order.");
                return RedirectToAction(nameof(MyOrders));
            }

            if (!success)
                SetError("Unable to delete your order right now.");
            else
                SetSuccess("Your order was deleted successfully.");

            return RedirectToAction(nameof(MyOrders));
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadOrderFormLookupsAsync();
            return View(new CreateOrderRequestViewModel());
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderRequestViewModel model)
        {
            if (User.IsInRole("Customer"))
            {
                var customerId = GetCustomerId();

                if (!customerId.HasValue)
                    return RedirectToAction("AccessDenied", "Account");

                model.CustomersId = customerId.Value;
                ModelState.Remove(nameof(CreateOrderRequestViewModel.CustomersId));
            }

            if (!ModelState.IsValid)
            {
                await LoadOrderFormLookupsAsync();
                return View(model);
            }

            bool success;
            try
            {
                success = await _orderServ.CreateOrderAsync(model);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "A system error occurred while creating the order.");
                await LoadOrderFormLookupsAsync();
                return View(model);
            }

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to create order. Please verify customer, product selection, stock, and quantities.");
                await LoadOrderFormLookupsAsync();
                return View(model);
            }

            SetSuccess("Order created successfully.");
            if (User.IsInRole("Admin"))
                return RedirectToAction(nameof(Index));

            return RedirectToAction(nameof(MyOrders));
        }
    }
}
