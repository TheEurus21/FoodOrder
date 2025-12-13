using System.ComponentModel.DataAnnotations;

namespace FoodOrder.DTOs.User
{
    public class UserRequest
    {
        [Required(ErrorMessage ="Name should be entered")]
        public string Username { get; set; }
        [EmailAddress(ErrorMessage ="Invalid email format")]
        public string Email { get; set; }
        public string Password { get; set; }     
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsOwner { get; set; }        
    }
}
