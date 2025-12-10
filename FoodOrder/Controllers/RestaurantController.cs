using Microsoft.AspNetCore.Mvc;
using FoodOrder.Models;
using FoodOrder.DTOs;
using FoodOrder.Repositories.Common;
using FoodOrder.DTOs.Restaurant;
using FoodOrder.DTOs.Food;
using FoodOrder.DTOs.FoodCategory;
using Microsoft.AspNetCore.Authorization;

namespace FoodOrder.Controllers
{
    [ApiController]
    [Route("api/restaurants")]
    [Authorize]
    public class RestaurantController : CommonController
    {
        private readonly ApplicationRepository<Restaurant> _repo;

        public RestaurantController(ApplicationRepository<Restaurant> repo)
        {
            _repo = repo;
        }
        
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<RestaurantResponse>>> GetAll()
        {
            var restaurants = await _repo.GetAllAsync();
            return restaurants.Select(MapToResponse).ToList();
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<RestaurantResponse>>GetById(int id)
        {
            var restaurant =await _repo.GetByIdAsync(id); 
                if (restaurant == null) return NotFound();
            return MapToResponse(restaurant);
        }
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<RestaurantResponse>> Create(RestaurantRequest request)
        {
            var userId = GetCurrentUserId();

            var restaurant = new Restaurant
            {
                Name = request.Name,
                Address = request.Address,
                UserId = userId,
                RestaurantCode = Guid.NewGuid()
            };

            var created = await _repo.AddAsync(restaurant);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> Update(int id, RestaurantRequest request)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var userId = GetCurrentUserId();
            if (existing.UserId != userId) return Forbid();

            existing.Name = request.Name;
            existing.Address = request.Address;
            existing.Categories = request.Categories.Select(c => new FoodCategory
            {
                Name = c.Name,
                Foods = c.Foods.Select(f => new Food
                {
                    Name = f.Name,
                    Description = f.Description,
                    Price = f.Price,
                    Type = f.Type
                }).ToList()
            }).ToList();
            await _repo.UpdateAsync(existing);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
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



    


    
