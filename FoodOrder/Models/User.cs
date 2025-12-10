namespace FoodOrder.Models
{
    public class User
    {
        public int Id { get; set; }

       
        public string Username { get; set; }     
        public string Email { get; set; }        
        public string Password { get; set; }  

        
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public UserRole Role { get; set; }


        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Restaurant> Restaurants { get; set; } = new List<Restaurant>();
        
    }
}
