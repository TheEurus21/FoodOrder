using FoodOrder.Application.DTOs.Food;

namespace FoodOrder.Application.DTOs.FoodCategory
{
    public class FoodCategoryResponse
    {
        public string Name { get; set; }
        public List<FoodResponse> Foods { get; set; } = new();
    }
}
