
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Order.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
