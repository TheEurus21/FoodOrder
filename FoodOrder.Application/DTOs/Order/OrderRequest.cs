using System.ComponentModel.DataAnnotations;

namespace FoodOrder.Application.DTOs.Order
{
    public class OrderRequest
    {
        [Required(ErrorMessage = "Restaurant's name must be entered")]
        public string RestaurantName { get; set; }
        public List<int> FoodIds { get; set; } = new();
        public string ?Notes { get; set; }
        public string PhoneNumber { get; set; }
    }
}
