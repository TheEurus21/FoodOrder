namespace FoodOrder.DTOs.User
{
    public class UserResponse
    {
        public int Id { get; set; }              
        public string Username { get; set; }     
        public string Email { get; set; }       


        public string FullName { get; set; }     
        public string PhoneNumber { get; set; }  

     
        public List<UserOrderResponse> Orders { get; set; } = new();
        public List<UserReviewResponse> Reviews { get; set; } = new();
    }
}
