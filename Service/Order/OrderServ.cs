using Inventory_Order.Repository.OrderRepository;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Order.Service.Order
{
    public class OrderServ : IOrderServ
    {
        private readonly IOrderRepo _orderRepo;
        public OrderServ(IOrderRepo orderRepo)
        {
            _orderRepo = orderRepo;
        }


    }   
}
