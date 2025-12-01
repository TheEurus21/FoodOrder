namespace FoodOrder.DTOs
{
    public class FoodCategoryResponse
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<FoodResponse> Foods { get; set; } = new();
    }
}
