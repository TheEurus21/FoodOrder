namespace FoodOrder.DTOs.Review
{
    public class ReviewResponse
    {
        public string Comment { get; set; }
        public int Rating { get; set; }
        public int RestaurantId { get; set; }
        public int? FoodId { get; set; }
    }
}
