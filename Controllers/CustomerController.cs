using Microsoft.AspNetCore.Mvc;

namespace Inventory_Order.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
