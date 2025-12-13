using FoodOrder.DTOs.Food;
using System.ComponentModel.DataAnnotations;

namespace FoodOrder.DTOs.FoodCategory
{
    public class FoodCategoryRequest
    {
        [Required(ErrorMessage ="Name must be entered")]
        public string ?Name { get; set; }
        public List<FoodRequest> Foods { get; set; } = new();

    }
}
