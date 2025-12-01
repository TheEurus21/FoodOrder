using Microsoft.AspNetCore.Mvc;
using FoodOrder.Services;
using FoodOrder.Models;

namespace FoodOrder.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController:ControllerBase
    {

        [HttpGet]
        public List<Order> GetAll()
        {
            return OrderRepository.Orders;
        }
        [HttpPost]
        public Order Add(Order order)
        {
            order.Id = OrderRepository.Orders.Count + 1;
            OrderRepository.Orders.Add(order);
            return order;
           
        }
    }
}
