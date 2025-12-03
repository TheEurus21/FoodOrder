namespace FoodOrder.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int? FoodId { get; set; }
        public Food? Food { get; set; }

    }
}
