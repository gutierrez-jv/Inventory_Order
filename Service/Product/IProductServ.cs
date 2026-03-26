using Inventory_Order.Models.Database;

namespace Inventory_Order.Service.Product
{
    // Interface defining the contract for product-related business logic operations
    public interface IProductServ
    {
        Task<List<ProductTb>> GetAllProductsAsync(); // Asynchronously retrieves a list of all products
        Task<ProductTb?> GetProductByIdAsync(int id); // Asynchronously retrieves a product by its unique identifier, returning null if not found
        Task<bool> CreateProductAsync(ProductTb product); // Asynchronously creates a new product based on the provided product data, returning true if successful
        Task<bool> UpdateProductAsync(ProductTb product); // Asynchronously updates an existing product based on the provided product data, returning true if successful
        Task<bool> DeleteProductAsync(int id); // Asynchronously deletes a product based on its unique identifier, returning true if successful
    }
}