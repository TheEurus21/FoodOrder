
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

        [HttpGet("{id}")]
        public async Task<ActionResult<RestaurantResponse>>GetById(int id)
        {
            var restaurant =await _context.Restaurants
                .Include(r => r.Categories)
                .ThenInclude(c=>c.Foods)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (restaurant == null) return NotFound();
            return MapToResponse(restaurant);
        }
        [HttpPost]
        public async Task<ActionResult<RestaurantResponse>>Create(Restaurant restaurant)
        {
            restaurant.RestaurantCode = Guid.NewGuid();

            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new {id=restaurant.Id},MapToResponse(restaurant));
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, Restaurant restaurant)
        {
            if (id != restaurant.Id) return BadRequest();
            _context.Entry(restaurant).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RestaurantExists(id)) return NotFound();
            }
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult>Delete(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return NotFound();
            _context.Restaurants.Remove(restaurant);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        private bool RestaurantExists(int id)
        {
            return _context.Restaurants.Any(e => e.Id == id);
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



    


    
