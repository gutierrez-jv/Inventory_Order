using Microsoft.AspNetCore.Mvc;

namespace Inventory_Order.Repository.OrderRepository
{
    public class OrderRepo : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
