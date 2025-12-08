namespace FoodOrder.DTOs.Review
{
    public class ReviewRequest
    {
        public string FoodName { get; set; }         
        public int Rating { get; set; }          
        public string? Comment { get; set; }     
    }
}
