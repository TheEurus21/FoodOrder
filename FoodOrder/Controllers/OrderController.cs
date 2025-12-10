using FoodOrder.DTOs.Order;
using FoodOrder.Models;
using FoodOrder.Repositories.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrderController : CommonController
    {
        private readonly ApplicationRepository<Order> _repo;

        public OrderController(ApplicationRepository<Order> repo)
        {
            _repo = repo;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<OrderResponse>>> GetAll()
        {
            var orders = await _repo.GetAllAsync();
            return Ok(orders.Select(MapToResponse).ToList());
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<OrderResponse>> Create(OrderRequest request)
        {
            var order = new Order
            {
                RestaurantName = request.RestaurantName,
                UserId = GetCurrentUserId(), 
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending, 
                                              
            };

            var created = await _repo.AddAsync(order);
            return Ok(MapToResponse(created));
        }
        [HttpDelete("latest")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> DeleteLatest()
        {
            var currentUserId = GetCurrentUserId();
            var existing = await _repo.GetLatestOrderByUserAsync(currentUserId);

            if (existing == null) return NotFound();

            var deleted = await _repo.DeleteAsync(existing.Id);
            if (!deleted) return NotFound();

            return NoContent();
        }


        [HttpPut("latest")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> UpdateLatest(OrderRequest request)
        {
            var currentUserId = GetCurrentUserId();
            var existing = await _repo.GetLatestOrderByUserAsync(currentUserId);
            if (existing == null) return NotFound();
            existing.RestaurantName = request.RestaurantName;
            existing.Note = request.Notes;
            await _repo.UpdateAsync(existing);
            return NoContent();
        }


        private OrderResponse MapToResponse(Order order)
        {
            return new OrderResponse
            {
                Status = order.Status.ToString(), 
                CreatedAt = order.CreatedAt       
            };
        }
    }
}
