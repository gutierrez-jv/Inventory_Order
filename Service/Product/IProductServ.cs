using Inventory_Order.Models.Database;

namespace Inventory_Order.Service.Product
{
    public interface IProductServ
    {
        Task<List<ProductTb>> GetAllProductsAsync();
        Task<ProductTb?> GetProductByIdAsync(int id);
        Task<bool> CreateProductAsync(ProductTb product);
        Task<bool> UpdateProductAsync(ProductTb product);
        Task<bool> DeleteProductAsync(int id);
    }
}