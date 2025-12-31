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
using System.Text.Json;

namespace FoodOrder.API.Controllers
{
    [ApiController]
    [Route("api/categories")]

    public class CategoryController : CommonController
    {
        private readonly ICategoryRepository _repo;
        private readonly IDistributedCache _cache;
        private readonly IRestaurantRepository _restaurantRepo;
        private readonly ILogger<RestaurantController> _logger;

        public CategoryController(ICategoryRepository repo, IDistributedCache cache, IRestaurantRepository restaurantRepo, ILogger<RestaurantController> logger)
        {
            _repo = repo;
            _cache = cache;
            _restaurantRepo = restaurantRepo;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<FoodCategoryResponse>>> GetAll()
        {
            _logger.LogInformation("request for all categories");
            const string cacheKey = "categories_all";

            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("cache hit key {cachekey}", cacheKey);
                var cachedCategories = System.Text.Json.JsonSerializer
                    .Deserialize<List<FoodCategoryResponse>>(cachedData);
                return Ok(cachedCategories);
            }
            _logger.LogInformation("Cache miss for key {CacheKey}", cacheKey);

            var categories = await _repo.GetAllAsync();
            _logger.LogInformation("Fetched {Count} categories from repository", categories.Count());
            var response = categories.Select(MapToResponse).ToList();

            var serialized = System.Text.Json.JsonSerializer.Serialize(response);
            await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });
            _logger.LogInformation("Cached {Count} categories with key {CacheKey}", response.Count, cacheKey);
            return Ok(response);
        }
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<FoodCategory>> GetById(int id)
        {
            string cacheKey =$"category_{id}";
            var cached=await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("cache hit for id {categoryId}", id);
               var response = JsonSerializer.Deserialize<FoodCategoryResponse>(cached);
                return Ok(response);
            }
            var existing = await _repo.GetByIdAsync(id); 
            if (existing == null) return NotFound(); 
            var mapped = MapToResponse(existing); 
            var serialized = JsonSerializer.Serialize(mapped);
            await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions 
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });
            _logger.LogInformation("Cached category ID {CategoryId}", id); 
            return Ok(mapped);
        }

        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<FoodCategoryResponse>> Create(FoodCategoryRequest request)
        {
            _logger.LogInformation("Creating category {Name} for restaurant {RestaurantId}", request.Name, request.RestaurantId);
            var restaurant = await _restaurantRepo.GetByIdAsync(request.RestaurantId);
            if (restaurant == null) 
            {
                _logger.LogWarning("Invalid RestaurantId {RestaurantId} provided when creating category", request.RestaurantId);
                return BadRequest("Invalid RestaurantId");
            }

            
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
            _logger.LogInformation("Updating category with ID {CategoryId}", id);
            var currentUserId = GetCurrentUserId();
            var existingCategory = await _repo.GetByIdAsync(id);
            if (existingCategory == null) return NotFound();
            
            if (existingCategory.Restaurant.UserId != currentUserId) return Forbid(); 
   
            existingCategory.Name = request.Name;
            existingCategory.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(existingCategory);
            await _cache.RemoveAsync($"category_{id}"); 
            await _cache.RemoveAsync("categories_all");
            _logger.LogInformation("Invalidated caches for category {CategoryId}", id);
            return NoContent();
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting category with ID {CategoryId}", id);
            var currentUserId = GetCurrentUserId();
            var existingCategory = await _repo.GetByIdAsync(id);
            if (existingCategory == null) 
            {
                _logger.LogWarning("Category with ID {CategoryId} not found for deletion", id); 
                return NotFound(); 
            }
            if (existingCategory.Restaurant.UserId != currentUserId) 
            { 
                _logger.LogWarning("User {UserId} attempted to delete category {CategoryId}", currentUserId, id);
                return Forbid(); 
            }
            var deleted = await _repo.DeleteAsync(id);
            await _cache.RemoveAsync($"category_{id}");
            await _cache.RemoveAsync("categories_all");
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
