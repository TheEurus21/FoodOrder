using FoodOrder.DTOs.Food;

namespace FoodOrder.DTOs.FoodCategory
{
    public class FoodCategoryRequest
    {
        public string? Name { get; set; }
        public List<FoodRequest> Foods { get; set; } = new();

    }
}
