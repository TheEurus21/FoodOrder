namespace FoodOrder.Domain.Entities
{
    public class FoodCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }


        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }


        public ICollection<Food> Foods { get; set; } = new List<Food>();


        public int UserId { get; set; }
        public User User { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }
}
