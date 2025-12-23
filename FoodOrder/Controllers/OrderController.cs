using FoodOrder.Application.DTOs.Order;
using FoodOrder.Domain.Entities;
using FoodOrder.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace FoodOrder.API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrderController : CommonController
    {
        private readonly IOrderRepository _repo;
        private readonly IDistributedCache _cache;
        public OrderController(IOrderRepository repo, IDistributedCache cache)
        {
            _repo = repo;
            _cache = cache;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<OrderResponse>>> GetAll()
        {
            const string cacheKey = "orders_all";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                var cachedOrders = System.Text.Json.JsonSerializer.
                    Deserialize<List<OrderResponse>>(cachedData);
                return Ok(cachedOrders);
            }
            var orders = await _repo.GetAllAsync();
            var response = orders.Select(MapToResponse).ToList();
            var serialized = System.Text.Json.JsonSerializer.Serialize(response);
            await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });
            return Ok(response);
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderResponse>> GetById(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            return Ok(existing);

        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<OrderResponse>> Create(OrderRequest request)
        {
            var order = new Order
            {
                RestaurantName = request.RestaurantName,
                UserId = GetCurrentUserId(),
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                Notes = request.Notes
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

            existingOrder.Notes = request.Notes;
            await _repo.UpdateAsync(existingOrder);
            return NoContent();
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> Delete(int id)
        {
            var currentUserId = GetCurrentUserId();
            var existingOrder = await _repo.GetByIdAsync(id);
            if (existingOrder == null) return NotFound();
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
