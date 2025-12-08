using FoodOrder.DTOs.Food;
using FoodOrder.DTOs.FoodCategory;
using FoodOrder.Models;
using FoodOrder.Repositories.Common;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationRepository<FoodCategory> _repo;

        public CategoryController(ApplicationRepository<FoodCategory> repo)
        {
            _repo = repo;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
        }

        [HttpGet]
        public async Task<ActionResult<List<FoodCategoryResponse>>> GetAll()
        {
            var categories = await _repo.GetAllAsync();
            return Ok(categories.Select(MapToResponse).ToList());
        }

        [HttpPost]
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            if (existing.UserId != currentUserId) return Forbid();

            var deleted = await _repo.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, FoodCategoryRequest request)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            if (existing.UserId != currentUserId) return Forbid();

            existing.Name = request.Name;

            if (request.Foods != null && request.Foods.Any())
            {
                existing.Foods = request.Foods.Select(f => new Food
                {
                    Name = f.Name,
                    Description = f.Description,
                    Price = f.Price,
                    Type = f.Type
                }).ToList();
            }

            await _repo.UpdateAsync(existing);
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
