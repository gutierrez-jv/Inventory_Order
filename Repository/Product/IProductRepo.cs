using Inventory_Order.Models.Database;

namespace Inventory_Order.Repository.ProductRepository
{
    // Defines the contract for product-related database operations, including retrieval, addition, updating, and deletion of products
    public interface IProductRepo
    {
        Task<List<ProductTb>> GetAllProductsAsync(); // Return a list of all products in the database
        Task<ProductTb?> GetProductByIdAsync(int id); // Return a specific product by its ID, or null if not found
        Task<ProductTb?> GetProductByBarcodeAsync(string barcode); // Return a specific product by its barcode, or null if not found
        Task AddProductAsync(ProductTb product); // Add a new product to the database
        Task UpdateProductAsync(ProductTb product); // Update an existing product's information in the database
        Task DeleteProductAsync(int id); // Delete a product from the database by its ID, or mark it as inactive if it has related orders
        Task<bool> HasOrderItemsAsync(int productId); // Check if a product is associated with any order items, which can be used to determine if it can be safely deleted
    }
}