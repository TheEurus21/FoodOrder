using FoodOrder.Application.DTOs.Restaurant;
using FoodOrder.Application.Interfaces;
using FoodOrder.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace FoodOrder.API.Controllers
{
    [ApiController]
    [Route("api/restaurants")]
    [Authorize]
    public class RestaurantController : CommonController
    {
        private readonly IRestaurantRepository _repo;
        private readonly IDistributedCache _cache;
        private readonly ILogger<RestaurantController> _logger;
        public RestaurantController(IRestaurantRepository repo, IDistributedCache cache, ILogger<RestaurantController> logger)
        {
            _repo = repo;
            _cache = cache;
            _logger = logger;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<RestaurantResponse>>> GetAll()
        {
            const string cacheKey = "restaurants_all";
            _logger.LogInformation("Requested all restaurants");

            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Cache hit for key {CacheKey}", cacheKey);

                var cachedRestaurants = System.Text.Json.JsonSerializer
                    .Deserialize<List<RestaurantResponse>>(cachedData);

                return Ok(cachedRestaurants);
            }

            _logger.LogInformation("Cache miss for key {CacheKey}", cacheKey);

            var restaurants = await _repo.GetAllAsync();
            _logger.LogInformation("Fetched {Count} restaurants from repository", restaurants.Count());

            var response = restaurants.Select(MapToResponse).ToList();

            var serialized = System.Text.Json.JsonSerializer.Serialize(response);
            await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });

            _logger.LogInformation("Cached {Count} restaurants with key {CacheKey}", response.Count, cacheKey);

            return Ok(response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<RestaurantResponse>> GetById(int id)
        {
            string cacheKey = $"restaurant_{id}";
            var cached=await _cache.GetStringAsync(cacheKey);
            if(cached!= null)
            {
                _logger.LogInformation("Cache hit for restaurant ID {RestaurantId}", id);
                var response= JsonSerializer.Deserialize<RestaurantResponse>(cached); 
                return Ok(response);
            }
            else
            {
                _logger.LogInformation("Cache miss for restaurant ID {RestaurantId}", id);
            }
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            var mapped=MapToResponse(existing);
            var serialized= JsonSerializer.Serialize(mapped); 
            await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            { 
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            }); 
            _logger.LogInformation("Cached restaurant ID {RestaurantId}", id);

            return Ok(mapped);
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
            _logger.LogInformation("Creating restaurant {Name} for user {UserId}", request.Name, userId);

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
            await _cache.RemoveAsync($"restaurant_{id}");
            if (existingRestaurant.UserId != currentUserId) return Forbid();
            _logger.LogInformation("Updating restaurant with ID {RestaurantId}", id);

           
            if (existingRestaurant.UserId != currentUserId)
            {
                _logger.LogWarning("User {UserId} attempted to update restaurant {RestaurantId}", currentUserId, id);
                return Forbid();
            }

            existingRestaurant.Name = request.Name;
            existingRestaurant.Address = request.Address;
            await _repo.UpdateAsync(existingRestaurant);
            await _cache.RemoveAsync($"restaurant_{id}");
            await _cache.RemoveAsync("restaurants_all");
            _logger.LogInformation("Invalidated caches for restaurant {RestaurantId}", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> Delete(int id)
        {
            var currentUserId = GetCurrentUserId();
            var existingRestaurant = await _repo.GetByIdAsync(id);

            if (existingRestaurant == null) return NotFound();
            await _cache.RemoveAsync($"restaurant_{id}");
            if (existingRestaurant.UserId != currentUserId) return Forbid();
            _logger.LogInformation("Deleting restaurant with ID {RestaurantId}", id);
            if (existingRestaurant.UserId != currentUserId)
            {
                _logger.LogWarning("User {UserId} attempted to delete restaurant {RestaurantId}", currentUserId, id);
                return Forbid();
            }

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
