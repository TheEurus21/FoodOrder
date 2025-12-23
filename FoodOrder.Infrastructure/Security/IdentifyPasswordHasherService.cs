using FoodOrder.Domain.Entities;
using Microsoft.AspNetCore.Identity;

public class IdentityPasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<User> _hasher =
        new(); public HashMethod HashMethod => HashMethod.IdentityV3;
    public bool VerifyPassword(User user, string password)
    {
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result != PasswordVerificationResult.Failed;
    }
    public string HashPassword(string password)
    {
        return _hasher.HashPassword(null, password);
    }
}