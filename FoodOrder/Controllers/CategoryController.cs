using FoodOrder.DTOs.Food;
using FoodOrder.DTOs.FoodCategory;
using FoodOrder.Models;
using FoodOrder.Repositories.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Controllers
{
    [ApiController]
    [Route("api/categories")]

    public class CategoryController : CommonController
    {
        private readonly ApplicationRepository<FoodCategory> _repo;

        public CategoryController(ApplicationRepository<FoodCategory> repo)
        {
            _repo = repo;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<FoodCategoryResponse>>> GetAll()
        {
            var categories = await _repo.GetAllAsync();
            return Ok(categories.Select(MapToResponse).ToList());
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
