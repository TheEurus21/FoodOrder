using System.ComponentModel.DataAnnotations;

namespace FoodOrder.DTOs.Review
{
    public class ReviewRequest
    {
        public int FoodId { get; set; }
        [Range(1,5,ErrorMessage ="Rating range is 1-5")]
        public int Rating { get; set; }          
        public string ?Comment { get; set; }     
    }
}
