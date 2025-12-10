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

        [HttpDelete]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> Delete()
        {
            var currentUserId = GetCurrentUserId();
            var existing = await _repo.GetByOwnerIdAsync(currentUserId);
            if (existing == null) return NotFound();

            var deleted = await _repo.DeleteAsync(currentUserId);
            if (!deleted) return NotFound();

            return NoContent();
        }

        [HttpPut]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> Update(FoodCategoryRequest request)
        {
            var currentUserId = GetCurrentUserId();
            var existingCategory = await _repo.GetByOwnerIdAsync(currentUserId);

            if (existingCategory == null) return NotFound();

            existingCategory.Name = request.Name;
            if (request.Foods != null && request.Foods.Any())
            {
                existingCategory.Foods = request.Foods.Select(f => new Food
                {
                    Name = f.Name,
                    Description = f.Description,
                    Price = f.Price,
                    Type = f.Type
                }).ToList();
            }

            await _repo.UpdateAsync(existingCategory); 
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
