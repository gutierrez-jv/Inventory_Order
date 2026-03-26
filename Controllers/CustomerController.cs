using Inventory_Order.Models.Database;
using Inventory_Order.Service.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Order.Controllers
{
    // This controller manages customer records (Create, Read, Update, Delete).
    [Authorize(Roles = "Admin")] // Only users in the "Admin" role can access these actions
    public class CustomerController : Controller
    {
        // Service for handling customer-related business logic 
        private readonly ICustomerServ _customerServ;

        public CustomerController(ICustomerServ customerServ)
        {
            _customerServ = customerServ;
        }

        // Display a list of all customers
        public async Task<IActionResult> Index()
        {
            var customers = await _customerServ.GetAllCustomersAsync();
            return View(customers);
        }

        // Show the form to edit an existing customer
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerServ.GetCustomerByIdAsync(id); // Retrieve the customer by ID using the service layer

            if (customer == null) return NotFound(); // If the customer does not exist, return a 404 Not Found response

            return View(customer); // Pass the customer data to the view for editing
        }

        // Process the form submission to update an existing customer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerTb customer)
        {
            // Validate the model state; if it's not valid, return the view with the current customer data to display validation errors
            if (!ModelState.IsValid)
                return View(customer); // Return the view with the current customer data to display validation errors

            bool success; // Flag to track if the update operation was successful

            try
            {
                success = await _customerServ.UpdateCustomerAsync(customer); // Attempt to update the customer using the service layer
            }
            catch
            {
                // If an exception occurs, add a generic error message to the model state and return the view
                ModelState.AddModelError(string.Empty, "A system error occurred while updating the customer.");
                return View(customer);
            }

            // If the update operation was not successful (e.g., due to validation issues), add an error message and return the view
            if (!success)
            {
                // If the update operation was not successful (e.g., due to validation issues), add an error message and return the view
                ModelState.AddModelError(string.Empty, "Unable to update customer. Please verify first name and last name.");
                return View(customer);
            }

            // If successful, set a success message in TempData and redirect to the Index action
            TempData["SuccessMessage"] = "Customer updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Process the request to delete a customer
        public async Task<IActionResult> Delete(int id)
        {
            bool success; // Flag to track if the delete operation was successful
            try
            {
                success = await _customerServ.DeleteCustomerAsync(id); // Attempt to delete the customer using the service layer
            }
            catch
            {
                // If an exception occurs, set a generic error message in TempData and redirect to the Index action
                TempData["ErrorMessage"] = "A system error occurred while deleting the customer.";
                return RedirectToAction(nameof(Index));
            }

            // If the delete operation was not successful (e.g., the record may no longer exist), set an error message; otherwise, set a success message
            if (!success) TempData["ErrorMessage"] = "Unable to delete customer. The record may no longer exist.";
            else
                TempData["SuccessMessage"] = "Customer deleted successfully."; // If successful, set a success message in TempData

            return RedirectToAction(nameof(Index));
        }
    }
}
