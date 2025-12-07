namespace FoodOrder.Models
{
    public class Restaurant
    {
        public int Id { get; set; }
        public Guid RestaurantCode {  get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public ICollection<FoodCategory> Categories { get; set; } = new List<FoodCategory>();
        public ICollection<Order> Orders { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
