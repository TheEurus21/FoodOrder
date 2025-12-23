using FoodOrder.Application.DTOs.Food;

namespace FoodOrder.Application.DTOs.FoodCategory
{
    public class FoodCategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int RestaurantId { get; set; }
        public List<FoodResponse> Foods { get; set; } = new();
    }
}
