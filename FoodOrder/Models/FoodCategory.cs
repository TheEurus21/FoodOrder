namespace FoodOrder.Models
{
    public class FoodCategory
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int RestaurantId {  get; set; }
        public Restaurant Restaurant { get; set; }
        public ICollection<Food> Foods { get; set; }=new List<Food>();

    }
}
