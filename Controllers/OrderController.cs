using Microsoft.AspNetCore.Mvc;

namespace Inventory_Order.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
