using FoodOrder.Application.DTOs.FoodCategory;
using System.ComponentModel.DataAnnotations;

namespace FoodOrder.Application.DTOs.Restaurant
{
    public class RestaurantRequest
    {
        [Required(ErrorMessage = "Restaurant's name must be entered")]
        public string? Name { get; set; }
        public string? Address { get; set; }

    }
}
