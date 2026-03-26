using Inventory_Order.Models.Database;
using Inventory_Order.Service.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Order.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CustomerController : Controller
    {
        private readonly ICustomerServ _customerServ;

        public CustomerController(ICustomerServ customerServ)
        {
            _customerServ = customerServ;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _customerServ.GetAllCustomersAsync();
            return View(customers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerTb customer)
        {
            if (!ModelState.IsValid)
                return View(customer);

            bool success;
            try
            {
                success = await _customerServ.AddCustomerAsync(customer);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "A system error occurred while creating the customer.");
                return View(customer);
            }

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to create customer. Please verify first name and last name.");
                return View(customer);
            }

            TempData["SuccessMessage"] = "Customer created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerServ.GetCustomerByIdAsync(id);

            if (customer == null)
                return NotFound();

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerTb customer)
        {
            if (!ModelState.IsValid)
                return View(customer);

            bool success;
            try
            {
                success = await _customerServ.UpdateCustomerAsync(customer);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "A system error occurred while updating the customer.");
                return View(customer);
            }

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to update customer. Please verify first name and last name.");
                return View(customer);
            }

            TempData["SuccessMessage"] = "Customer updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            bool success;
            try
            {
                success = await _customerServ.DeleteCustomerAsync(id);
            }
            catch
            {
                TempData["ErrorMessage"] = "A system error occurred while deleting the customer.";
                return RedirectToAction(nameof(Index));
            }

            if (!success)
                TempData["ErrorMessage"] = "Unable to delete customer. The record may no longer exist.";
            else
                TempData["SuccessMessage"] = "Customer deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
