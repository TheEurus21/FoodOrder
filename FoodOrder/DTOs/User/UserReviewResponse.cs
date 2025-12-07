namespace FoodOrder.DTOs.User
{
    public class UserReviewResponse
    {
        public string Comment { get; set; }
        public int Rating { get; set; }          
        public int RestaurantId { get; set; }    
        public int? FoodId { get; set; }         
    }
}
