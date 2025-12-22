using FoodOrder.Application.DTOs.Order;
using FoodOrder.Application.DTOs.Review;

namespace FoodOrder.Application.DTOs.User
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }


        public string FullName { get; set; }
        public string PhoneNumber { get; set; }


        public List<OrderResponse> Orders { get; set; } = new();
        public List<ReviewResponse> Reviews { get; set; } = new();
    }
}
