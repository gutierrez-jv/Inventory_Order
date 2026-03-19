using Microsoft.AspNetCore.Mvc;

namespace Inventory_Order.Service.Product
{
    public class ProductServ : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
