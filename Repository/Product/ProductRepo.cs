using Inventory_Order.Models.Database;
using Microsoft.EntityFrameworkCore; 

namespace Inventory_Order.Repository.ProductRepository
{
    // Handles direct database operations for products
    public class ProductRepo : IProductRepo
    {
        // Database context used to access product data
        private readonly InventoryOrderDbContext _context;

        public ProductRepo(InventoryOrderDbContext context)
        {
            _context = context;
        }

        // Retrieves all products 
        public async Task<List<ProductTb>> GetAllProductsAsync()
        {
            return await _context.ProductTbs
                .AsNoTracking()
                .ToListAsync();
        }

        // Retrieves a specific product by ID 
        public async Task<ProductTb?> GetProductByIdAsync(int id)
        {
            return await _context.ProductTbs
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductsId == id);
        }

        // Retrieves a specific product by barcode 
        public async Task<ProductTb?> GetProductByBarcodeAsync(string barcode)
        {
            return await _context.ProductTbs
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Barcode == barcode);
        }

        // Adds a new product to the database
        public async Task AddProductAsync(ProductTb product)
        {
            await _context.ProductTbs.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        // Updates an existing product's basic details
        public async Task UpdateProductAsync(ProductTb product)
        {
            // Find the existing product in the database
            var existingProduct = await _context.ProductTbs
                .FirstOrDefaultAsync(p => p.ProductsId == product.ProductsId);

            // Stop if product does not exist
            if (existingProduct == null)
                return;

            // Update product fields
            existingProduct.Name = product.Name;
            existingProduct.Type = product.Type;
            existingProduct.Quantity = product.Quantity;
            existingProduct.Price = product.Price;
            existingProduct.Barcode = product.Barcode;

            // Save changes
            await _context.SaveChangesAsync();
        }

        // Deletes a product from the database
        public async Task DeleteProductAsync(int id)
        {
            var existingProduct = await _context.ProductTbs.FindAsync(id);
            if (existingProduct != null)
            {
                _context.ProductTbs.Remove(existingProduct); // Remove the product from the database
                await _context.SaveChangesAsync(); // save changes to the database
            }
        }

        // Checks if the product is already referenced by order items
        public async Task<bool> HasOrderItemsAsync(int productId)
        {
            return await _context.OrderItemTbs.AnyAsync(i => i.ProductsId == productId); 
        }
    }
}