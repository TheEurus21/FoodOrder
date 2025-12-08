namespace FoodOrder.DTOs.User
{
    public class UserRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }     
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsOwner { get; set; }        
    }
}
