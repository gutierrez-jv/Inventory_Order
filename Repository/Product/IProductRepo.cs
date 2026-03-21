using Inventory_Order.Models.Database;

namespace Inventory_Order.Repository.ProductRepository
{
    public interface IProductRepo
    {
        Task<List<ProductTb>> GetAllProductsAsync();
        Task<ProductTb?> GetProductByIdAsync(int id);
        Task<ProductTb?> GetProductByBarcodeAsync(string barcode);
        Task AddProductAsync(ProductTb product);
        Task UpdateProductAsync(ProductTb product);
        Task DeleteProductAsync(int id);
    }
}