namespace FoodOrder.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }    
        public ICollection<Order> Orders { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

    }
}
