using Microsoft.AspNetCore.Mvc;

namespace Inventory_Order.Repository.ProductRepository
{
    public class ProductRepo : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
