using Inventory_Order.Service.Order;
using Inventory_Order.ViewModels.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Order.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderServ _orderServ;

        public OrderController(IOrderServ orderServ)
        {
            _orderServ = orderServ;
        }

        private int? GetCustomerId()
        {
            var value = User.FindFirst("CustomerId")?.Value;

            if (int.TryParse(value, out int customerId))
                return customerId;

            return null;
        }

        // ADMIN: View all orders
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var orders = await _orderServ.GetAllOrdersAsync();
            return View(orders);
        }

        // ADMIN: View one order
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderServ.GetOrderByIdAsync(id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // ADMIN: Mark completed
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            await _orderServ.CompleteOrderAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // ADMIN: Cancel
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            await _orderServ.CancelOrderAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // CUSTOMER: View own orders
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyOrders()
        {
            var customerId = GetCustomerId();

            if (!customerId.HasValue)
                return RedirectToAction("AccessDenied", "Account");

            var orders = await _orderServ.GetOrdersByCustomerIdAsync(customerId.Value);
            return View(orders);
        }

        // CUSTOMER: Create own order
        [Authorize(Roles = "Customer")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderRequestViewModel model)
        {
            var customerId = GetCustomerId();

            if (!customerId.HasValue)
                return RedirectToAction("AccessDenied", "Account");

            model.CustomersId = customerId.Value;

            if (!ModelState.IsValid)
                return View(model);

            var success = await _orderServ.CreateOrderAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to create order.");
                return View(model);
            }

            return RedirectToAction(nameof(MyOrders));
        }
    }
}