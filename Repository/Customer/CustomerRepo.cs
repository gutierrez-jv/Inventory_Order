using Inventory_Order.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Order.Repository.CustomerRepository
{
    // Handles direct database operations for customers
    public class CustomerRepo : ICustomerRepo
    {
        // Database context used to access customer data
        private readonly InventoryOrderDbContext _context;

        public CustomerRepo(InventoryOrderDbContext context)
        {
            _context = context;
        }

        // Retrieves all customers 
        public async Task<List<CustomerTb>> GetAllCustomersAsync()
        {
            return await _context.CustomerTbs
                .Include(c => c.UserTbs)
                .AsNoTracking()
                .ToListAsync();
        }

        // Retrieves a specific customer by ID
        public async Task<CustomerTb?> GetCustomerByIdAsync(int id)
        {
            return await _context.CustomerTbs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == id);
        }

        // Adds a new customer to the database
        public async Task AddCustomerAsync(CustomerTb customer)
        {
            await _context.CustomerTbs.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        // Updates an existing customer's basic details
        public async Task UpdateCustomerAsync(CustomerTb customer)
        {
            // Find the existing customer in the database
            var existingCustomer = await _context.CustomerTbs
                .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

            // Stop if customer does not exist
            if (existingCustomer == null)
                return;

            // Update customer fields
            existingCustomer.FirstName = customer.FirstName;
            existingCustomer.LastName = customer.LastName;
            existingCustomer.IsActive = customer.IsActive;

            // Save changes
            await _context.SaveChangesAsync();
        }

        // Deletes a customer or performs a soft delete if related orders exist
        // If the customer has existing orders, use soft delete by marking them inactive
        // so the system keeps order history and avoids breaking related records.
        // If the customer has no orders, use hard delete because no transaction history depends on that customer.
        public async Task DeleteCustomerAsync(int id)
        {
            // Retrieve customer with related orders and user accounts
            var customer = await _context.CustomerTbs
                .Include(c => c.OrderTbs) // Load related orders
                .Include(c => c.UserTbs) // Load related users
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            // Stop if customer not found
            if (customer == null)
                return;

            // If customer has existing orders
            if (customer.OrderTbs.Count > 0)
            {
                // Soft delete: mark customer as inactive
                customer.IsActive = false;

                // Also deactivate all related user accounts
                foreach (var user in customer.UserTbs)
                {
                    user.IsActive = false;
                }
            }
            else
            {
                // If no orders, fully delete customer and related users
                _context.UserTbs.RemoveRange(customer.UserTbs);
                _context.CustomerTbs.Remove(customer);
            }

            // Save changes
            await _context.SaveChangesAsync();
        }
    }
}