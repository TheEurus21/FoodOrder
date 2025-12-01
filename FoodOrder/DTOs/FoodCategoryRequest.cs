namespace FoodOrder.DTOs
{
    public class FoodCategoryRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<FoodRequest> Foods { get; set; } = new();

    }
}
