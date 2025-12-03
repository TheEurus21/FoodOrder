using FoodOrder.Models;

namespace FoodOrder.DTOs.Food
{
    public class FoodRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public FoodType Type { get; set; }
    }
}
