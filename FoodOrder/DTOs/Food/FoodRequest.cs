using FoodOrder.Models;
using System.ComponentModel.DataAnnotations;

namespace FoodOrder.DTOs.Food
{
    public class FoodRequest
    {
        [Required(ErrorMessage ="Name is required")]
        [StringLength(50,ErrorMessage ="Name cannot be this long")]
        public string Name { get; set; }

        public string ?Description { get; set; }
        [Range(1,1000000,ErrorMessage ="Price must be more than 0")]
        public decimal Price { get; set; }
        public FoodType Type { get; set; }
    }
}
