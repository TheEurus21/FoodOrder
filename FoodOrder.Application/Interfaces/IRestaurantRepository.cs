using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces
{
    public interface IRestaurantRepository
    {
        Task<IEnumerable<Restaurant>> GetAllAsync();
        Task<Restaurant?> GetByIdAsync(int id);
        Task<Restaurant> AddAsync(Restaurant restaurant);
        Task UpdateAsync(Restaurant restaurant);
        Task<bool> DeleteAsync(int id);
        Task<int>CountByOwner(int userId);
    }
}
