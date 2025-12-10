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

        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<RestaurantResponse>> Create(RestaurantRequest request)
        {
            var userId = GetCurrentUserId();

            var restaurant = new Restaurant
            {
                Name = request.Name,
                Address = request.Address
            };

            var created = await _repo.AddAsync(restaurant);
            return Created($"api/restaurants/{created.Id}", MapToResponse(created));

        }
        [HttpPut("{restaurantName}")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> Update(string restaurantName, RestaurantRequest request)
        {
            var userId = GetCurrentUserId();
            var existing = await _repo.GetByNameAndOwnerAsync(restaurantName, userId);

            if (existing == null) return NotFound();

            existing.Address = request.Address;
            existing.Name = request.Name;

            await _repo.UpdateAsync(existing);
            return NoContent();
        }
        [HttpDelete("{restaurantName}")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> Delete(string restaurantName)
        {
            var userId = GetCurrentUserId();
            var existing = await _repo.GetByNameAndOwnerAsync(restaurantName, userId);

            if (existing == null) return NotFound();

            var deleted = await _repo.DeleteAsync(existing.Id);
            if (!deleted) return NotFound();

            return NoContent();
        }

        private RestaurantResponse MapToResponse(Restaurant restaurant)
        {
            return new RestaurantResponse
            {
                Name = restaurant.Name,
                Address = restaurant.Address,
            };
        }
    }
}