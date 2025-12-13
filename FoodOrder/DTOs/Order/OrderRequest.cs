using System.ComponentModel.DataAnnotations;

namespace FoodOrder.DTOs.Order
{
    public class OrderRequest
    {
        [Required(ErrorMessage ="Restaurant's name must be entered")]
        public string RestaurantName { get; set; }
        public List<int> FoodIds { get; set; } = new();
        public string ?Notes { get; set; }             
    }
}
