using Inventory_Order.Models.Database;

namespace Inventory_Order.Repository.ProductRepository
{
    public interface IProductRepo
    {
        Task<List<ProductTb>> GetAllProducts();
        Task<ProductTb?> GetProductById(int id);
        Task<ProductTb?> GetProductByBarcode(string barcode);
        void AddProduct(ProductTb product);
        Task<ProductTb?> UpdateProduct(ProductTb product);
        void DeleteProduct(int id);
    }
}
