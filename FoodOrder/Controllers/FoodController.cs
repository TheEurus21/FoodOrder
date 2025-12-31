using Microsoft.AspNetCore.Mvc;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using FoodOrder.Application.DTOs.FoodCategory;
using Microsoft.Extensions.Caching.Distributed;
using Azure;
using FoodOrder.Application.DTOs.Food;
using FoodOrder.Application.Interfaces;
using FoodOrder.Domain.Entities;
using System.Text.Json;

namespace FoodOrder.API.Controllers
{
    [ApiController]
    [Route("api/food")]

    public class FoodController : CommonController
    {
        private readonly IFoodRepository _repo;
        private readonly IDistributedCache _cache;
        private readonly ICategoryRepository _categoryRepo;
        private readonly ILogger<FoodController> _logger;
        public FoodController(IFoodRepository repo, IDistributedCache cache, ICategoryRepository categoryRepo,ILogger<FoodController>logger)
        {
            _repo = repo;
            _cache = cache;
            _categoryRepo = categoryRepo;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<FoodResponse>>> GetAll()
        {
            const string cacheKey = "food_all";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                var cachedFoods = System.Text.Json.JsonSerializer
                    .Deserialize<List<FoodResponse>>(cachedData);
                return Ok(cachedFoods);
            }
            var foods = await _repo.GetAllAsync();
            var response = foods.Select(MapToResponse).ToList();

            var serialized = System.Text.Json.JsonSerializer.Serialize(response);
            await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });

            return Ok(response);

        }
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<FoodResponse>> GetById(int id)
        {
            string cacheKey = $"food_{id}";
            var cached=await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("cache hit for food id {FoodId}", id);
                var response=JsonSerializer.Deserialize<FoodResponse>(cached);
                return Ok(response);
            }
            else
            {
                _logger.LogInformation("miss cache for Food id {FoodId}", id);
            }
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            var mapped = MapToResponse(existing);
            var serialized = JsonSerializer.Serialize(mapped);
            await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });
            _logger.LogInformation("Cached Food ID {FoodId}", id);

            return Ok(mapped);
        }

        [HttpPost("{categoryId}/foods")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<FoodResponse>> Create(int categoryId, FoodRequest request)
        {
            var currentUserId = GetCurrentUserId();
            var category = await _categoryRepo.GetByIdAsync(categoryId); 
            if (category == null) return NotFound(); 
            if (category.Restaurant.UserId != currentUserId) return Forbid(); 
            var food = new Food 
            {
                Name = request.Name, 
                Description = request.Description,
                Price = request.Price,
                Type = request.Type,
                FoodCategoryId = categoryId 
            };

            var created = await _repo.AddAsync(food);
            return Created($"api/foods/{created.Id}", MapToResponse(created));

        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> Update(int id, FoodRequest request)
        {
            var currentUserId = GetCurrentUserId();
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            if (existing.Category.Restaurant.UserId != currentUserId) return Forbid();
            existing.Name = request.Name;
            existing.Description = request.Description;
            existing.Price = request.Price;
            existing.Type = request.Type;
            await _repo.UpdateAsync(existing);
            await _cache.RemoveAsync($"food_{id}");
            await _cache.RemoveAsync("food_all");
            _logger.LogInformation("Invalidated caches for food ID {FoodId}", id);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> Delete(int id)
        {
            var currentUserId = GetCurrentUserId();
            var existingFood = await _repo.GetByIdAsync(id);
            if (existingFood == null) return NotFound();
            if (existingFood.Category.Restaurant.UserId != currentUserId) return Forbid();
            var deleted = await _repo.DeleteAsync(id);
            await _cache.RemoveAsync($"food_{id}");
            await _cache.RemoveAsync("food_all");
            _logger.LogInformation("Invalidated caches for food ID {FoodId}", id);
            if (!deleted) return NotFound();
            return NoContent();
        }
        private FoodResponse MapToResponse(Food food)
        {
            return new FoodResponse
            {
                Id= food.Id,
                Name = food.Name,
                Price = food.Price,
                Description = food.Description,
                Type = food.Type,
                CategoryId=food.FoodCategoryId
            };
        }
    }
}