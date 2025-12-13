using Microsoft.AspNetCore.Mvc;
using FoodOrder.Models;
using FoodOrder.Repositories.Common;
using FoodOrder.DTOs.Food;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;

namespace FoodOrder.Controllers
{
    [ApiController]
    [Route("api/food")]

    public class FoodController : CommonController
    {
        private readonly ApplicationRepository<Food> _repo;
        public FoodController(ApplicationRepository<Food> repo)
        {
            _repo = repo;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<FoodResponse>>> GetAll()
        {
            var foods=await _repo.GetAllAsync();
            return foods.Select(MapToResponse).ToList();

        }
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<FoodResponse>>GetById(int id)
        {
            var existing= await _repo.GetByIdAsync(id);
            if(existing==null)return NotFound();  
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
            if (existing.Category.Restaurant.UserId != currentUserId)return Forbid();
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
            var existingFood=await _repo.GetByIdAsync(id);
            if(existingFood == null) return NotFound();
            if (existingFood.Category.Restaurant.UserId != currentUserId)return Forbid();
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