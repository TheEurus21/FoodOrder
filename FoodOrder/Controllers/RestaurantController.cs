using Microsoft.AspNetCore.Mvc;
using FoodOrder.Models;
using FoodOrder.DTOs;
using FoodOrder.Repositories.Common;
using FoodOrder.DTOs.Restaurant;
using FoodOrder.DTOs.Food;
using FoodOrder.DTOs.FoodCategory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

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
            var existing = await _repo.GetByIdAsync(id);

            if (existing == null)return NotFound();

            return Ok(MapToResponse(existing));
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
                UserId = userId
            };

            var created = await _repo.AddAsync(restaurant);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));

        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> Update(int id, RestaurantRequest request)
        {
            var currentUserId=GetCurrentUserId();
            var existingRestaurant=await _repo.GetByIdAsync(id);
            if(existingRestaurant == null) return NotFound();
            if (existingRestaurant.UserId != currentUserId) return Forbid();
            existingRestaurant.Name = request.Name;
            existingRestaurant.Address= request.Address;
           await _repo.UpdateAsync(existingRestaurant);
            return NoContent();
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> Delete(int id)
        {
            var currentUserId = GetCurrentUserId();
            var existingRestaurant = await _repo.GetByIdAsync(id);

            if (existingRestaurant == null) return NotFound();
            if(existingRestaurant.UserId!= currentUserId) return Forbid();
            var deleted = await _repo.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }

        private RestaurantResponse MapToResponse(Restaurant restaurant)
        {
            return new RestaurantResponse
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                Address = restaurant.Address,
            };
        }
    }
}