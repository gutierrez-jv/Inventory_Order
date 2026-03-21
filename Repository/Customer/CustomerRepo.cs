using Inventory_Order.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Order.Repository.CustomerRepository
{
    public class CustomerRepo : ICustomerRepo
    {
        private readonly InventoryOrderDbContext _context;

        public CustomerRepo(InventoryOrderDbContext context)
        {
            _context = context;
        }

        public async Task<List<CustomerTb>> GetAllCustomersAsync()
        {
            return await _context.CustomerTbs.AsNoTracking().ToListAsync();
        }

        public async Task<CustomerTb?> GetCustomerByIdAsync(int id)
        {
            return await _context.CustomerTbs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == id);
        }

        public async Task AddCustomerAsync(CustomerTb customer)
        {
            await _context.CustomerTbs.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCustomerAsync(CustomerTb customer)
        {
            var existingCustomer = await _context.CustomerTbs
                .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

            if (existingCustomer == null)
                return;

            existingCustomer.FirstName = customer.FirstName;
            existingCustomer.LastName = customer.LastName;
            existingCustomer.IsActive = customer.IsActive;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteCustomerAsync(int id)
        {
            var customer = await _context.CustomerTbs
                .Include(c => c.OrderTbs)
                .Include(c => c.UserTbs)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null)
                return;

            if (customer.OrderTbs.Count > 0)
            {
                customer.IsActive = false;

                foreach (var user in customer.UserTbs)
                {
                    user.IsActive = false;
                }
            }
            else
            {
                _context.UserTbs.RemoveRange(customer.UserTbs);
                _context.CustomerTbs.Remove(customer);
            }

            await _context.SaveChangesAsync();
        }
    }
}
