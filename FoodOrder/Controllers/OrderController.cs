using FoodOrder.DTOs.User;
using FoodOrder.Models;
using FoodOrder.Repositories.Common;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationRepository<Order> _repo;

        public OrderController(ApplicationRepository<Order> repo)
        {
            _repo = repo;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
        }

        [HttpGet]
        public async Task<ActionResult<List<UserOrderResponse>>> GetAll()
        {
            var orders = await _repo.GetAllAsync();
            return Ok(orders.Select(MapToResponse).ToList());
        }

        [HttpPost]
        public async Task<ActionResult<UserOrderResponse>> Create(Order order)
        {
            order.UserId = GetCurrentUserId(); 
            var created = await _repo.AddAsync(order);
            return Ok(MapToResponse(created));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            if (existing.UserId != currentUserId) return Forbid();

            var deleted = await _repo.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, Order order)
        {
            if (id != order.Id) return BadRequest();

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            if (existing.UserId != currentUserId) return Forbid();

            await _repo.UpdateAsync(order);
            return NoContent();
        }

        private UserOrderResponse MapToResponse(Order order)
        {
            return new UserOrderResponse
            {
                Status = order.Status.ToString(), 
                CreatedAt = order.CreatedAt       
            };
        }
    }
}
