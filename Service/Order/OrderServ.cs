using Microsoft.AspNetCore.Mvc;

namespace Inventory_Order.Service.Order
{
    public class OrderServ : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
