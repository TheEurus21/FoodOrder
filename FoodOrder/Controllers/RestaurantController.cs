using FoodOrder.Application.DTOs.Restaurant;
using FoodOrder.Application.Interfaces;
using FoodOrder.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace FoodOrder.API.Controllers
{
    [ApiController]
    [Route("api/restaurants")]
    [Authorize]
    public class RestaurantController : CommonController
    {
        private readonly IRestaurantRepository _repo;
        private readonly IDistributedCache _cache;

        public RestaurantController(IRestaurantRepository repo, IDistributedCache cache)
        {
            _repo = repo;
            _cache = cache;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<RestaurantResponse>>> GetAll()
        {
            const string cacheKey = "restaurants_all";

            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                var cachedRestaurants = System.Text.Json.JsonSerializer
                    .Deserialize<List<RestaurantResponse>>(cachedData);
                return Ok(cachedRestaurants);
            }

            var restaurants = await _repo.GetAllAsync();
            var response = restaurants.Select(MapToResponse).ToList();

            var serialized = System.Text.Json.JsonSerializer.Serialize(response);
            await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });

            return Ok(response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<RestaurantResponse>> GetById(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            return Ok(MapToResponse(existing));
        }

        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<RestaurantResponse>> Create(RestaurantRequest request)
        {
            var userId = GetCurrentUserId();

            var restaurant = new Restaurant
            {
                Name = request.Name,
                Address = request.Address,
                UserId = userId
            };

            var created = await _repo.AddAsync(restaurant);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> Update(int id, RestaurantRequest request)
        {
            var currentUserId = GetCurrentUserId();
            var existingRestaurant = await _repo.GetByIdAsync(id);
            if (existingRestaurant == null) return NotFound();
            if (existingRestaurant.UserId != currentUserId) return Forbid();

            existingRestaurant.Name = request.Name;
            existingRestaurant.Address = request.Address;
            await _repo.UpdateAsync(existingRestaurant);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> Delete(int id)
        {
            var currentUserId = GetCurrentUserId();
            var existingRestaurant = await _repo.GetByIdAsync(id);

            if (existingRestaurant == null) return NotFound();
            if (existingRestaurant.UserId != currentUserId) return Forbid();

            var deleted = await _repo.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }

        private RestaurantResponse MapToResponse(Restaurant restaurant)
        {
            return new RestaurantResponse
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                Address = restaurant.Address,
            };
        }
    }
}
