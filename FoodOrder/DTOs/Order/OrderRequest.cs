namespace FoodOrder.DTOs.Order
{
    public class OrderRequest
    {
        public string RestaurantName { get; set; }
        public List<int> FoodIds { get; set; } = new();
        public string? Notes { get; set; }             
    }
}
