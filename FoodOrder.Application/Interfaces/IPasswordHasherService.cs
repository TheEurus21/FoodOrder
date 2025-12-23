using FoodOrder.Domain.Entities;

public interface IPasswordHasherService
{
    bool VerifyPassword(User user, string password);
    string HashPassword(string password);
    HashMethod HashMethod { get; }
}
