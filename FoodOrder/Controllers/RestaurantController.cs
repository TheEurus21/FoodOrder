
using Microsoft.AspNetCore.Mvc;
using FoodOrder.Models;
using FoodOrder.DTOs;
using FoodOrder.Repositories.Common;
using FoodOrder.DTOs.Restaurant;
using FoodOrder.DTOs.Food;
using FoodOrder.DTOs.FoodCategory;

namespace FoodOrder.Controllers
{
    [ApiController]
    [Route("api/restaurants")]
    public class RestaurantController : ControllerBase
    {
        private readonly ApplicationRepository<Restaurant> _repo;

        public RestaurantController(ApplicationRepository<Restaurant> repo)
        {
            _repo = repo;
        }
        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
        }
        [HttpGet]
        public async Task<ActionResult<List<RestaurantResponse>>> GetAll()
        {
            var restaurants = await _repo.GetAllAsync();
            return restaurants.Select(MapToResponse).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RestaurantResponse>>GetById(int id)
        {
            var restaurant =await _repo.GetByIdAsync(id); 
                if (restaurant == null) return NotFound();
            return MapToResponse(restaurant);
        }
        [HttpPost]
        public async Task<ActionResult<RestaurantResponse>>Create(Restaurant restaurant)
        {
            var userId = GetCurrentUserId(); 
            restaurant.UserId = userId;
            restaurant.RestaurantCode = Guid.NewGuid();
            var created=await _repo.AddAsync(restaurant);
            return CreatedAtAction(nameof(GetById), new {id=created.Id},MapToResponse(created));

        }
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, Restaurant restaurant)
        {
            if (id != restaurant.Id) return BadRequest();
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var userId = GetCurrentUserId();
            if (existing.UserId != userId) return Forbid();
            await _repo.UpdateAsync(restaurant);
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult>Delete(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var userId = GetCurrentUserId();
            if (existing.UserId != userId) return Forbid();

            var deleted = await _repo.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
        
        private RestaurantResponse MapToResponse(Restaurant restaurant)
        {
            return new RestaurantResponse
            {
                Name = restaurant.Name,
                Address = restaurant.Address,
                Categories = restaurant.Categories.Select(c => new FoodCategoryResponse
                {
                    Name = c.Name,
                    Description = c.Description,
                    Foods = c.Foods.Select(f => new FoodResponse
                    {
                        Name = f.Name,
                        Description = f.Description,
                        Price = f.Price,
                        Type = f.Type
                    }).ToList()
                }).ToList()
            };
        }
    }
}



    


    
