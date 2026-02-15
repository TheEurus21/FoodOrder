using FoodOrder.Application.DTOs.Order;
using FoodOrder.Domain.Entities;
using FoodOrder.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using MassTransit;
using FoodOrder.Contracts.Events;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using Amqp.Listener;
using FoodOrder.Application.Services;

namespace FoodOrder.API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrderController : CommonController
    {
        private readonly IOrderRepository _repo;
        private readonly IDistributedCache _cache;
        private readonly ILogger<OrderController> _logger;
        public readonly IPublishEndpoint _publishEndpoint;
        private readonly RestaurantService _restaurantService;

        public OrderController(IOrderRepository repo, IDistributedCache cache,ILogger<OrderController>logger,IPublishEndpoint publishEndpoint,RestaurantService restaurantService)
        {
            _repo = repo;
            _cache = cache;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _restaurantService = restaurantService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<OrderResponse>>> GetAll()
        {
            const string cacheKey = "orders_all";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Cache hit for key {CacheKey}", cacheKey);
                var cachedOrders = System.Text.Json.JsonSerializer.
                    Deserialize<List<OrderResponse>>(cachedData);
                return Ok(cachedOrders);
            }
            _logger.LogInformation("Cache miss for key {CacheKey}", cacheKey);

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
            return Ok(MapToResponse(existing));

        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<OrderResponse>> Create(OrderRequest request)
        {
            int? userId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var parsedUserId))
            {
                userId = parsedUserId;
            }
            var readyBy = DateTimeOffset.UtcNow.AddMinutes(30);
            var todayCount = await _repo.CountTodayOrderPerRestaurant(request.RestaurantId);
            _restaurantService.ValidateOwnerCanPlaceOrder(todayCount);

            var order = new Order
            {
                RestaurantId = request.RestaurantId,
                CorrelationId = Guid.Parse(HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString()),
                OrderCorrelationId = Guid.NewGuid(),
                RestaurantName = request.RestaurantName,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                Notes = request.Notes,
                PhoneNumber = request.PhoneNumber
            };

            var created = await _repo.AddAsync(order);
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            await _publishEndpoint.Publish(new OrderCreated(
                CorrelationId: Guid.Parse(correlationId),
                OrderId: created.Id,
                PhoneNumber: request.PhoneNumber,
                ReadyBy: readyBy
            ));


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
            await _cache.RemoveAsync("orders_all");
            return NoContent();
        }
        [HttpPut("{id}/status")]
        [AllowAnonymous]
        public async Task<ActionResult> ChangeStatus(int id, [FromBody] ChangeOrderStatusRequest request)
        {
            var existingOrder = await _repo.GetByIdAsync(id);
            if (existingOrder == null) return NotFound();

            existingOrder.Status = request.Status;
            await _repo.UpdateAsync(existingOrder);

            await _cache.RemoveAsync("orders_all");

            if (request.Status == OrderStatus.Completed)
                await _publishEndpoint.Publish(new OrderReady(
          existingOrder.CorrelationId,
          existingOrder.Id,
          existingOrder.PhoneNumber,
          DateTimeOffset.UtcNow
                ));

            return Ok(MapToResponse(existingOrder));
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
            await _cache.RemoveAsync("orders_all");
            if (!deleted) return NotFound();

            return NoContent();
        }
        private OrderResponse MapToResponse(Order order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                RestaurantName = order.RestaurantName,
                Notes = order.Notes,
                Status = order.Status,
                PhoneNumber = order.PhoneNumber,
                CreatedAt = order.CreatedAt
            };
        }

    }
}
