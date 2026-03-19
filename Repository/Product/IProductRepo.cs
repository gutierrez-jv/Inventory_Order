using Inventory_Order.Models.Database;

namespace Inventory_Order.Repository.ProductRepository
{
    public interface IProductRepo
    {
        List<ProductTb> GetAllProducts();
        ProductTb? GetProductById(int id);
        ProductTb? GetProductByBarcode(string barcode);
        void AddProduct(ProductTb product);
        void UpdateProduct(ProductTb product);
        void DeleteProduct(int id);
    }
}
