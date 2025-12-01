
using Microsoft.AspNetCore.Mvc;
using FoodOrder.Services;
using FoodOrder.Models;
using FoodOrder.DTOs;
using System.Runtime.CompilerServices;
using FoodOrder.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Controllers
{
    [ApiController]
    [Route("api/restaurants")]
    public class RestaurantController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RestaurantController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<List<RestaurantResponse>>> GetAll()
        {
            var restaurants = await _context.Restaurants
                .Include(r => r.Categories)
                    .ThenInclude(c => c.Foods)
                .ToListAsync();

            return restaurants.Select(MapToResponse).ToList();
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



    


    
