namespace FoodOrder.Application.DTOs.Order
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public string RestaurantName { get; set; }
        public string Notes { get; set; }
        public OrderStatus Status { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
