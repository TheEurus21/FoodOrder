namespace FoodOrder.Models
{
    public class Food
    {
        public int Id { get; set; }
        public string ?Name { get; set; }
        public string ?Description { get; set; }
        public decimal Price { get; set; }
        public FoodType Type { get; set; } 
        public int FoodCategoryId {  get; set; }
        public FoodCategory Category { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    }
}
