using Microsoft.AspNetCore.Mvc;
using FoodOrder.Services;
using FoodOrder.Models;

namespace FoodOrder.Controllers
{
    [ApiController]
    [Route("api/food")]
    public class FoodController : ControllerBase
    {
        [HttpGet]
        public List<Food> GetAll()
        {
            return FoodRepository.Foods;

        }
        [HttpGet("byname/{name}")]
        public ActionResult<Food> GetByName(string name)
        {
            var foodName = FoodRepository.Foods.FirstOrDefault(f => f.Name == name);
            return foodName;

        }
        [HttpPost]
        public Food Add(Food food)
        {
            food.Id = FoodRepository.Foods.Count + 1;
            FoodRepository.Foods.Add(food);
            return food;
        }
    }
}