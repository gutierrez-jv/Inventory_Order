using Inventory_Order.Models.Database;

namespace Inventory_Order.Service.Product
{
    public interface IProductServ
    {
        IEnumerable<ProductTb> GetAllProducts();
        ProductTb? GetProductById(int id);
        bool CreateProduct(ProductTb product);
        bool UpdateProduct(ProductTb product);
        bool DeleteProduct(int id);

    }
}
