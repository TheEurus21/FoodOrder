using System.ComponentModel.DataAnnotations;

namespace FoodOrder.Application.DTOs.User
{
    public class UserRequest
    {
        [Required(ErrorMessage = "Name should be entered")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "Password must be at least 8 characters long")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; }

        public bool IsOwner { get; set; }
    }
}
