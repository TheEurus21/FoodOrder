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
        [HttpGet("{id}")]
        [Authorize(Roles ="Admin")]
        public async Task<ActionResult<OrderResponse>> GetById(int id)
        {
            var existing=await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            return Ok(existing);

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

        [HttpPut("{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> Update(int id, OrderRequest request)
        {
            var currentUserId = GetCurrentUserId();
            var existingOrder = await _repo.GetByIdAsync(id);
            if (existingOrder == null) return NotFound();
            if (existingOrder.UserId != currentUserId) return Forbid();

            existingOrder.Note = request.Notes;
            await _repo.UpdateAsync(existingOrder);
            return NoContent();
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> Delete(int id)
        {
            var currentUserId = GetCurrentUserId();
            var existingOrder = await _repo.GetByIdAsync(id);
            if(existingOrder == null) return NotFound();
            if (existingOrder.UserId != currentUserId) return Forbid();

            var deleted = await _repo.DeleteAsync(id);
            if (!deleted) return NotFound();

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
