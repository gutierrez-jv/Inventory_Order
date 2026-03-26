using Inventory_Order.Models.Database; 
using Inventory_Order.Repository.CustomerRepository; 

namespace Inventory_Order.Service.Customer
{
    // Handles customer-related business logic before calling the repository
    public class CustomerServ : ICustomerServ
    {
        // Repository used to access customer records
        private readonly ICustomerRepo _customerRepo;

        public CustomerServ(ICustomerRepo customerRepo)
        {
            _customerRepo = customerRepo;
        }

        // Retrieves all customers and returns an empty list if null
        public async Task<List<CustomerTb>> GetAllCustomersAsync()
        {
            var customers = await _customerRepo.GetAllCustomersAsync();
            return customers ?? new List<CustomerTb>(); // Return empty list if repository returns null
        }

        // Retrieves a customer by ID only if the ID is valid
        public async Task<CustomerTb?> GetCustomerByIdAsync(int id)
        {
            if (id <= 0) // Reject invalid IDs
                return null;

            return await _customerRepo.GetCustomerByIdAsync(id);
        }

        // Validates and adds a new customer
        public async Task<bool> AddCustomerAsync(CustomerTb customer)
        {
            if (customer == null) // Reject null input
                return false;

            // Reject missing first or last name
            if (string.IsNullOrWhiteSpace(customer.FirstName) ||
                string.IsNullOrWhiteSpace(customer.LastName))
                return false;

            await _customerRepo.AddCustomerAsync(customer); // Save valid customer
            return true;
        }

        // Validates and updates an existing customer
        public async Task<bool> UpdateCustomerAsync(CustomerTb customer)
        {
            if (customer == null) // Reject null input
                return false;

            if (customer.CustomerId <= 0) // Reject invalid customer ID
                return false;

            // Reject missing first or last name
            if (string.IsNullOrWhiteSpace(customer.FirstName) ||
                string.IsNullOrWhiteSpace(customer.LastName))
                return false;

            // Make sure the customer exists before updating
            var existingCustomer = await _customerRepo.GetCustomerByIdAsync(customer.CustomerId);
            if (existingCustomer == null)
                return false;

            await _customerRepo.UpdateCustomerAsync(customer); // Update valid existing customer
            return true;
        }

        // Validates and deletes an existing customer
        public async Task<bool> DeleteCustomerAsync(int id)
        {
            if (id <= 0) // Reject invalid IDs
                return false;

            // Make sure the customer exists before deleting
            var existingCustomer = await _customerRepo.GetCustomerByIdAsync(id);
            if (existingCustomer == null)
                return false;

            await _customerRepo.DeleteCustomerAsync(id); // Delete valid existing customer
            return true;
        }
    }
}