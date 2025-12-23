using FoodOrder.Application.DTOs.Food;
using System.ComponentModel.DataAnnotations;

namespace FoodOrder.Application.DTOs.FoodCategory
{
    public class FoodCategoryRequest
    {
        [Required(ErrorMessage = "Name must be entered")]
        public string? Name { get; set; }
        public int RestaurantId {  get; set; }

    }
}
