namespace FoodOrder.Models
{
    public class Restaurant
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string?  Address { get; set; }
        public ICollection<FoodCategory> Categories { get; set; }=new List<FoodCategory>();
        public ICollection<Review> Reviews { get; set; } =new List<Review>();   
    public ICollection<Order> Orders { get; set; }
    }
}
