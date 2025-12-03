using Microsoft.AspNetCore.Mvc;
using FoodOrder.Services;
using FoodOrder.Models;
using FoodOrder.Repositories.Common;
using FoodOrder.DTOs;

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
        [HttpPost]
        public async Task<ActionResult<FoodResponse>> Create(Food food)
        {
            var created = await _repo.AddAsync(food);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, Food food)
        {
            if (id !=food.Id) return BadRequest();
            await _repo.UpdateAsync(food);
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
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