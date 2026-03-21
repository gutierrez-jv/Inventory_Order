using Inventory_Order.Models.Database;
using Inventory_Order.Service.Customer;
using Inventory_Order.ViewModels.Customer;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Order.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerServ _customerService;

        public CustomerController(ICustomerServ customerService)
        {
            _customerService = customerService;
        }

        public IActionResult Index()
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Account");

            var customers = _customerService.GetAllCustomers();
            return View(customers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Account");

            return View(new CustomerFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CustomerFormViewModel model)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            var customer = new CustomerTb
            {
                CustomerId = model.CustomerId,
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                IsActive = model.IsActive
            };

            var success = _customerService.AddCustomer(customer);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to add customer. Check ID and required fields.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Customer added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Account");

            var success = _customerService.DeleteCustomer(id);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? "Customer deleted successfully."
                : "Unable to delete customer.";

            return RedirectToAction(nameof(Index));
        }

        private bool IsAdminAuthenticated()
        {
            return HttpContext.Session.GetString("IsAdminLoggedIn") == "true";
        }
    }
}
