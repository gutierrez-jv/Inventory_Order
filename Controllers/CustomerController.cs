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

            var success = await _customerServ.AddCustomerAsync(customer);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to create customer.");
                return View(customer);
            }

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

            var success = await _customerServ.UpdateCustomerAsync(customer);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to update customer.");
                return View(customer);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _customerServ.DeleteCustomerAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}