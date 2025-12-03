using FoodOrder.DTOs.FoodCategory;

namespace FoodOrder.DTOs.Restaurant
{
    public class RestaurantRequest
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public List<FoodCategoryRequest> Categories { get; set; } = new();
    }
}
