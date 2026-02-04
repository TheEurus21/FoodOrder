namespace FoodOrder.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public Guid CorrelationId { get; set; }
        public Guid OrderCorrelationId { get; set; }
        public int? RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public Restaurant? Restaurant { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public int? UserId { get; set; }
        public User User { get; set; }
        public string PhoneNumber {  get; set; }    
        public string Notes { get; set; }
        public decimal TotalPrice => Items.Sum(i => i.Quantity * i.Price);
    }

}
