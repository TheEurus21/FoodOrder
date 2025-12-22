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
        public CategoryController(ICategoryRepository repo, IDistributedCache cache)
        {
            _repo = repo;
            _cache = cache;
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

            if (existing == null)
                return NotFound();

            return Ok(existing);
        }

        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<FoodCategoryResponse>> Create(FoodCategoryRequest request)
        {
            var category = new FoodCategory
            {
                Name = request.Name,
                UserId = GetCurrentUserId(),
                Foods = request.Foods.Select(f => new Food
                {
                    Name = f.Name,
                    Description = f.Description,
                    Price = f.Price,
                    Type = f.Type
                }).ToList()
            };

            var created = await _repo.AddAsync(category);
            return Ok(MapToResponse(created));
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
                Name = category.Name,
                Foods = category.Foods.Select(f => new FoodResponse
                {
                    Name = f.Name,
                    Description = f.Description,
                    Price = f.Price,
                    Type = f.Type
                }).ToList()
            };
        }
    }
}
