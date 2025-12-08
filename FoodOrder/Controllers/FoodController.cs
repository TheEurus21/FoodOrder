using Microsoft.AspNetCore.Mvc;
using FoodOrder.Models;
using FoodOrder.Repositories.Common;
using FoodOrder.DTOs.Food;
using Azure.Core;

namespace FoodOrder.Controllers
{
    [ApiController]
    [Route("api/food")]
    public class FoodController : ControllerBase
    {
        private readonly ApplicationRepository<Food> _repo;
        public FoodController(ApplicationRepository<Food> repo)
        {
            _repo = repo;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
        }
        [HttpGet]
        public async Task<ActionResult<List<FoodResponse>>> GetAll()
        {
            var foods=await _repo.GetAllAsync();
            return foods.Select(MapToResponse).ToList();

        }
        [HttpGet("{id}")]
        public async Task<ActionResult<FoodResponse>>GetById(int id)
        {
            var food=await _repo.GetByIdAsync(id);
            if(food==null)return NotFound();
            return MapToResponse(food);
        }
        [HttpPost("/api/categories/{categoryId}/foods")]
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
                Type = request.Type,
                FoodCategoryId = categoryId
            };

            var created = await _repo.AddAsync(food);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }


        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, FoodRequest request)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            if (existing.Category.Restaurant.UserId != currentUserId)
                return Forbid();

            existing.Name = request.Name;
            existing.Description = request.Description;
            existing.Price = request.Price;
            existing.Type = request.Type;

            await _repo.UpdateAsync(existing);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var currentUserId = GetCurrentUserId();

            if (existing.Category.Restaurant.UserId != currentUserId)
                return Forbid();

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