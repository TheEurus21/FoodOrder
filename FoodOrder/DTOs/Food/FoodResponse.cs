using FoodOrder.Models;

namespace FoodOrder.DTOs.Food
{
    public class FoodResponse
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public FoodType Type { get; set; }
    }
}
