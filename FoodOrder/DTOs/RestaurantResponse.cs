namespace FoodOrder.DTOs
{
    public class RestaurantResponse
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public List<FoodCategoryResponse> Categories { get; set; } = new();
    }
}
