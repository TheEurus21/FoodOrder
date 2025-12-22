using Microsoft.AspNetCore.Mvc;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using FoodOrder.Application.DTOs.FoodCategory;
using Microsoft.Extensions.Caching.Distributed;
using Azure;
using FoodOrder.Application.DTOs.Food;
using FoodOrder.Application.Interfaces;
using FoodOrder.Domain.Entities;

namespace FoodOrder.API.Controllers
{
    [ApiController]
    [Route("api/food")]

    public class FoodController : CommonController
    {
        private readonly IFoodRepository _repo;
        private readonly IDistributedCache _cache;
        public FoodController(IFoodRepository repo, IDistributedCache cache)
        {
            _repo = repo;
            _cache = cache;
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
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            return Ok(existing);
        }

        [HttpPost("/api/categories/{categoryId}/foods")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<FoodResponse>> Create(int categoryId, FoodRequest request)
        {
            var currentUserId = GetCurrentUserId();
            var foods = await _repo.GetAllAsync();
            var category = foods
                .Select(f => f.Category)
                .FirstOrDefault(c => c.Id == categoryId);

            if (category == null) return NotFound();
            if (category.Restaurant.UserId != currentUserId)
                return Forbid();

            var food = new Food
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
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
            if (!deleted) return NotFound();
            return NoContent();
        }
        private FoodResponse MapToResponse(Food food)
        {
            return new FoodResponse
            {
                Name = food.Name,
                Price = food.Price,
                Description = food.Description,
                Type = food.Type
            };
        }
    }
}