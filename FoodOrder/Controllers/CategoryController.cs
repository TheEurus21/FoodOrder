using Azure;
using FoodOrder.Application.DTOs.FoodCategory;
using FoodOrder.Domain.Entities;
using FoodOrder.Application.DTOs.Food;
using FoodOrder.Application.DTOs.Restaurant;
using FoodOrder.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace FoodOrder.API.Controllers
{
    [ApiController]
    [Route("api/categories")]

    public class CategoryController : CommonController
    {
        private readonly ICategoryRepository _repo;
        private readonly IDistributedCache _cache;
        private readonly IRestaurantRepository _restaurantRepo;

        public CategoryController(ICategoryRepository repo, IDistributedCache cache, IRestaurantRepository restaurantRepo)
        {
            _repo = repo;
            _cache = cache;
            _restaurantRepo = restaurantRepo;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<FoodCategoryResponse>>> GetAll()
        {
            const string cacheKey = "categories_all";

            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                var cachedCategories = System.Text.Json.JsonSerializer
                    .Deserialize<List<FoodCategoryResponse>>(cachedData);
                return Ok(cachedCategories);
            }

            var categories = await _repo.GetAllAsync();
            var response = categories.Select(MapToResponse).ToList();

            var serialized = System.Text.Json.JsonSerializer.Serialize(response);
            await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });

            return Ok(response);
        }
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<FoodCategory>> GetById(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound(); 
            return Ok(MapToResponse(existing));
        }

        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<FoodCategoryResponse>> Create(FoodCategoryRequest request)
        {
            var restaurant = await _restaurantRepo.GetByIdAsync(request.RestaurantId);
            if (restaurant == null) return BadRequest("Invalid RestaurantId");

            var category = new FoodCategory
            {
                Name = request.Name,
                UserId = GetCurrentUserId(),
                RestaurantId = request.RestaurantId
            };

            var created = await _repo.AddAsync(category);
            var response = MapToResponse(created);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> Update(int id, FoodCategoryRequest request)
        {
            var currentUserId = GetCurrentUserId();
            var existingCategory = await _repo.GetByIdAsync(id);
            if (existingCategory == null) return NotFound();
            if (existingCategory.Restaurant.UserId != currentUserId)
                return Forbid();
            existingCategory.Name = request.Name;
            existingCategory.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(existingCategory);
            return NoContent();
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> Delete(int id)
        {
            var currentUserId = GetCurrentUserId();
            var existingCategory = await _repo.GetByIdAsync(id);
            if (existingCategory == null) return NotFound();
            if (existingCategory.Restaurant.UserId != currentUserId)
                return Forbid();
            var deleted = await _repo.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        private FoodCategoryResponse MapToResponse(FoodCategory category)
        {
            return new FoodCategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                RestaurantId = category.RestaurantId,
               Foods = category.Foods.Select(f => new FoodResponse 
               {
                   Id = f.Id,
                   Name = f.Name,
                   Description = f.Description,
                   Price = f.Price, 
                   Type = f.Type, 
                   CategoryId = f.FoodCategoryId
               }).ToList() 
            };
         }
    }
}
