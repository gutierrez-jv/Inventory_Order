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
    // This controller manages order records, including creating, viewing, updating, completing, and deleting orders.
    public class OrderController : Controller
    {
        // Service dependencies for handling order, customer, and product-related business logic
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

        // Gets the logged-in customer's ID from the user's claims
        private int? GetCustomerId()
        {
            var value = User.FindFirst("CustomerId")?.Value; // Read CustomerId claim from logged-in user

            if (int.TryParse(value, out int customerId)) // Convert claim value to int if possible
                return customerId;

            return null; // Return null if no valid customer ID exists
        }

        // Gets an order only if it belongs to the currently logged-in customer
        private async Task<OrderTb?> GetOwnOrderAsync(int id)
        {
            var customerId = GetCustomerId(); // Get logged-in customer's ID

            // If no valid customer ID exists, return null immediately to prevent unauthorized access
            if (!customerId.HasValue)
                return null;

            var order = await _orderServ.GetOrderByIdAsync(id); // Retrieve order by ID
            if (order == null || order.CustomersId != customerId.Value) // Check if order exists and belongs to this customer
                return null;

            return order; // Return order if valid and owned by current customer
        }

        // Loads customer and product dropdown data for order forms
        private async Task LoadOrderFormLookupsAsync(bool includeOutOfStockProducts = false)
        {
            var customers = await _customerServ.GetAllCustomersAsync(); // Get all customers
            var products = await _productServ.GetAllProductsAsync(); // Get all products

            var customerItems = new List<SelectListItem>(); // Convert customers to SelectListItem for dropdown

            // Only include active customers in the dropdown to prevent selection of inactive accounts
            foreach (var customer in customers)
            {
                if (!customer.IsActive) // Skip inactive customers
                    continue;

                // Display customer name and ID in the dropdown for clarity, using CustomerId as the value for form submission
                customerItems.Add(new SelectListItem
                {
                    // Use the customer ID as the selected value when the form is submitted
                    Value = customer.CustomerId.ToString(),

                    // Show the customer's full name and ID in the dropdown
                    Text = customer.FirstName + " " + customer.LastName + " (#" + customer.CustomerId + ")"
                });
            }

            // Convert products to SelectListItem for dropdown, optionally excluding out-of-stock items to prevent selection of unavailable products
            var productItems = new List<SelectListItem>();
            foreach (var product in products) // Loop through all products to create dropdown items
            {
                if (!includeOutOfStockProducts && product.Quantity <= 0) // Skip out-of-stock products unless explicitly allowed
                    continue;

                // Display product name, barcode, price, and stock level in the dropdown for clarity, using ProductsId as the value for form submission
                productItems.Add(new SelectListItem
                {
                    // Use the product ID as the selected value when the form is submitted
                    Value = product.ProductsId.ToString(),

                    // Show product name, barcode, price, and stock level in the dropdown
                    Text = product.Name + " (" + product.Barcode + ") - ₱" + product.Price.ToString("N2") + " | Stock: " + product.Quantity // Product display text
                });
            }

            ViewBag.Customers = customerItems; // Pass customer dropdown data to the view
            ViewBag.Products = productItems; // Pass product dropdown data to the view
        }

        // Stores a success message in TempData
        private void SetSuccess(string message) => TempData["SuccessMessage"] = message;

        // Stores an error message in TempData
        private void SetError(string message) => TempData["ErrorMessage"] = message;

        
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var orders = await _orderServ.GetAllOrdersAsync(); // Get all orders
            return View(orders); // Pass orders to the view
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderServ.GetOrderByIdAsync(id); // Get order by ID

            if (order == null)
                return NotFound(); // Return 404 if order does not exist

            return View(order); // Pass order details to the view
        }

        [Authorize(Roles = "Admin")]
        [HttpGet] // Displays the edit order form
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _orderServ.GetOrderByIdAsync(id); // Get order by ID

            if (order == null)
                return NotFound(); // Return 404 if order does not exist

            // ViewModel Explanation:
            //
            // A ViewModel is a class used to pass data between the Controller and the View (UI).
            // It is specifically designed for the page, containing only the fields needed for display or form input.
            //
            // ViewModels are located in the "ViewModels" folder (e.g., Inventory_Order.ViewModels.Order).
            // Each ViewModel file defines the structure of data used by a specific page or feature.
            //
            // In this project:
            // - The Controller creates or receives a ViewModel
            // - The View uses the ViewModel to display data or collect user input
            // - The ViewModel acts as a bridge between the UI and the Service layer
            //
            // This helps separate UI logic from database models and keeps the code clean and organized.

            // Create a new ViewModel object that will hold the order data for the edit form
            var model = new UpdateOrderRequestViewModel
            {
                // Copy the order ID from the database order into the ViewModel
                OrdersId = order.OrdersId,

                // Copy the customer ID from the database order into the ViewModel
                CustomersId = order.CustomersId,

                // Copy the order status from the database order into the ViewModel
                OrderStatus = order.OrderStatus,

                // Convert each order item from the database into a simpler item model for the form
                Items = order.OrderItemTbs.Select(i => new OrderItemRequestViewModel
                {
                    // Copy the product ID of the current order item into the item ViewModel
                    ProductsId = i.ProductsId,

                    // Copy the quantity of the current order item into the item ViewModel
                    Quantity = i.Quantity
                })
                // Convert the mapped item results into a list and assign it to Items
                .ToList()
            };

            await LoadOrderFormLookupsAsync(includeOutOfStockProducts: true); // Load dropdown data, including out-of-stock items for editing
            return View(model); // Return edit form with current order data
        }

        [Authorize(Roles = "Admin")] 
        [HttpPost] // Processes the submitted edit form
        [ValidateAntiForgeryToken]

        // The Edit POST action receives the updated order data from the form, validates it, and attempts to update the order through the service layer.
        // It handles success and error cases, providing feedback to the user and ensuring dropdown data is available if the form needs
        // to be redisplayed due to validation errors.
        public async Task<IActionResult> Edit(UpdateOrderRequestViewModel model) // The model parameter contains the data submitted from the edit form
        {
            if (!ModelState.IsValid) // Check if submitted form data is valid
            {
                await LoadOrderFormLookupsAsync(includeOutOfStockProducts: true); // Reload dropdown data before returning the view
                return View(model); // Return the same view with validation errors
            }

            bool success; // Variable to track if the update operation was successful
            try
            {
                success = await _orderServ.UpdateOrderAsync(model); // Attempt to update the order through the service layer
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "A system error occurred while updating the order.");
                await LoadOrderFormLookupsAsync(includeOutOfStockProducts: true); // Reload dropdown data before returning the view
                return View(model);
            }

            if (!success) // If update failed normally
            {
                ModelState.AddModelError(string.Empty, "Unable to update order. Please verify customer, products, and stock levels.");
                await LoadOrderFormLookupsAsync(includeOutOfStockProducts: true); // Reload dropdown data before returning the view
                return View(model);
            }

            SetSuccess("Order updated successfully."); // Store success message
            return RedirectToAction(nameof(Index)); // Redirect to order list
        }

        [Authorize(Roles = "Admin")] 
        [HttpPost] 
        [ValidateAntiForgeryToken]
        // The Complete action attempts to mark an order as completed.
        // It handles success and error cases, providing feedback to the user and ensuring they are redirected appropriately.
        public async Task<IActionResult> Complete(int id)
        {
            bool success; // Variable to track if the complete operation was successful

            try
            {
                success = await _orderServ.CompleteOrderAsync(id); // Attempt to mark order as completed
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

            return RedirectToAction(nameof(Index)); // Redirect to order list
        }

        [Authorize(Roles = "Admin")] 
        [HttpPost] 
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Cancel(int id)
        {
            bool success;

            try
            {
                success = await _orderServ.CancelOrderAsync(id); // Attempt to cancel the order
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

            return RedirectToAction(nameof(Index)); // Redirect to order list
        }

        [Authorize(Roles = "Admin")]
        [HttpPost] 
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Delete(int id)
        {
            bool success;
            try
            {
                success = await _orderServ.DeleteOrderAsync(id); // Attempt to delete the order
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

            return RedirectToAction(nameof(Index)); // Redirect to order list
        }

        [Authorize(Roles = "Customer")] // Only customers can view their own orders
        public async Task<IActionResult> MyOrders()
        {
            var customerId = GetCustomerId(); // Get logged-in customer's ID

            if (!customerId.HasValue)
                return RedirectToAction("AccessDenied", "Account"); // Redirect if no valid customer ID exists

            var orders = await _orderServ.GetOrdersByCustomerIdAsync(customerId.Value); // Get orders belonging to this customer
            return View(orders); // Pass orders to the view
        }

        [Authorize(Roles = "Customer")] // Only customers can view their own order details
        public async Task<IActionResult> MyOrderDetails(int id)
        {
            var order = await GetOwnOrderAsync(id); // Get the order only if it belongs to the current customer
            if (order == null)
                return RedirectToAction(nameof(MyOrders)); // Redirect if order is invalid or not owned by user

            return View(order); // Pass order details to the view
        }

        [Authorize(Roles = "Customer")]
        [HttpGet] // Displays the edit form for the customer's own order
        public async Task<IActionResult> MyOrderEdit(int id)
        {
            var order = await GetOwnOrderAsync(id); // Get the order only if it belongs to the current customer
            if (order == null)
                return RedirectToAction(nameof(MyOrders)); // Redirect if invalid

            // Convert database order data into a ViewModel for editing
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

            await LoadOrderFormLookupsAsync(includeOutOfStockProducts: true); // Load dropdown data for the form
            return View(model); // Return edit form with current order data
        }

        [Authorize(Roles = "Customer")] 
        [HttpPost] 
        [ValidateAntiForgeryToken]
        
        // The model parameter contains the updated order data submitted from the edit form
        public async Task<IActionResult> MyOrderEdit(UpdateOrderRequestViewModel model) 
        {
            var order = await GetOwnOrderAsync(model.OrdersId); // Make sure the order belongs to the current customer
            if (order == null)
                return RedirectToAction(nameof(MyOrders)); // Redirect if invalid

            model.CustomersId = order.CustomersId; // Force the customer ID to match the logged-in customer's order
            ModelState.Remove(nameof(UpdateOrderRequestViewModel.CustomersId)); // Remove validation for CustomersId since it was set manually

            if (!ModelState.IsValid) // Check if submitted form data is valid
            {
                await LoadOrderFormLookupsAsync(includeOutOfStockProducts: true); // Reload dropdown data
                return View(model); // Return same view with validation errors
            }

            bool success;
            try
            {
                success = await _orderServ.UpdateOrderAsync(model); // Attempt to update the order
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "A system error occurred while updating your order.");
                await LoadOrderFormLookupsAsync(includeOutOfStockProducts: true); // Reload dropdown data
                return View(model);
            }

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to update your order. Please verify products and stock levels.");
                await LoadOrderFormLookupsAsync(includeOutOfStockProducts: true); // Reload dropdown data
                return View(model);
            }

            SetSuccess("Your order was updated successfully."); // Store success message
            return RedirectToAction(nameof(MyOrders)); // Redirect to customer's order list
        }

        [Authorize(Roles = "Customer")] 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyOrderDelete(int id)
        {
            var order = await GetOwnOrderAsync(id); // Make sure the order belongs to the current customer
            if (order == null)
                return RedirectToAction(nameof(MyOrders)); // Redirect if invalid

            bool success;
            try
            {
                success = await _orderServ.DeleteOrderAsync(id); // Attempt to delete the order
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

            return RedirectToAction(nameof(MyOrders)); // Redirect to customer's order list
        }

        [Authorize(Roles = "Admin,Customer")] // Both admins and customers can access create order
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadOrderFormLookupsAsync(); // Load dropdown data for customers and products
            return View(new CreateOrderRequestViewModel()); // Return empty create order form
        }

        [Authorize(Roles = "Admin,Customer")] 
        [HttpPost] 
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Create(CreateOrderRequestViewModel model)
        {
            if (User.IsInRole("Customer")) // If the logged-in user is a customer
            {
                var customerId = GetCustomerId(); // Get current customer's ID

                if (!customerId.HasValue)
                    return RedirectToAction("AccessDenied", "Account"); // Redirect if no valid customer ID exists

                model.CustomersId = customerId.Value; // Automatically assign the logged-in customer's ID
                ModelState.Remove(nameof(CreateOrderRequestViewModel.CustomersId)); // Remove validation for CustomersId since it was set manually
            }

            if (!ModelState.IsValid)
            {
                await LoadOrderFormLookupsAsync(); // Reload dropdown data before returning the view
                return View(model); // Return same view with validation errors
            }

            bool success;
            try
            {
                success = await _orderServ.CreateOrderAsync(model); // Attempt to create the order
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "A system error occurred while creating the order.");
                await LoadOrderFormLookupsAsync(); // Reload dropdown data before returning the view
                return View(model);
            }

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to create order. Please verify customer, product selection, stock, and quantities.");
                await LoadOrderFormLookupsAsync(); // Reload dropdown data before returning the view
                return View(model);
            }

            SetSuccess("Order created successfully."); // Store success message

            if (User.IsInRole("Admin"))
                return RedirectToAction(nameof(Index)); // Redirect admin to all orders page

            return RedirectToAction(nameof(MyOrders)); // Redirect customer to their own orders page
        }
    }
}