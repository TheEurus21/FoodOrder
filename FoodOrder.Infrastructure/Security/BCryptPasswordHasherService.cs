using FoodOrder.Domain.Entities;
using BCrypt.Net;

public class BCryptPasswordHasherService : IPasswordHasherService
{
    public HashMethod HashMethod => HashMethod.BCrypt; 
    public bool VerifyPassword(User user, string password)
        => BCrypt.Net.BCrypt.Verify(password, user.PasswordHash); 
    public string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password); 
}