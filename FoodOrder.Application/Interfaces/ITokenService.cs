using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
