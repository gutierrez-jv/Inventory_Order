using Inventory_Order.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Order.Repository.ProductRepository
{
    public class ProductRepo : IProductRepo
    {
        private readonly InventoryOrderDbContext _context;

        public ProductRepo(InventoryOrderDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductTb>> GetAllProductsAsync()
        {
            return await _context.ProductTbs
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ProductTb?> GetProductByIdAsync(int id)
        {
            return await _context.ProductTbs
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductsId == id);
        }

        public async Task<ProductTb?> GetProductByBarcodeAsync(string barcode)
        {
            return await _context.ProductTbs
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Barcode == barcode);
        }

        public async Task AddProductAsync(ProductTb product)
        {
            await _context.ProductTbs.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(ProductTb product)
        {
            var existingProduct = await _context.ProductTbs
                .FirstOrDefaultAsync(p => p.ProductsId == product.ProductsId);

            if (existingProduct == null)
                return;

            existingProduct.Name = product.Name;
            existingProduct.Type = product.Type;
            existingProduct.Quantity = product.Quantity;
            existingProduct.Price = product.Price;
            existingProduct.Barcode = product.Barcode;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var existingProduct = await _context.ProductTbs.FindAsync(id);
            if (existingProduct != null)
            {
                _context.ProductTbs.Remove(existingProduct);
                await _context.SaveChangesAsync();
            }
        }
    }
}
