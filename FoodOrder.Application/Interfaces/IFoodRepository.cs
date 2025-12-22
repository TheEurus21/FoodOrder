using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces
{
    public interface IFoodRepository
    {
        Task<IEnumerable<Food>> GetAllAsync();
        Task<Food?> GetByIdAsync(int id);
        Task<Food> AddAsync(Food food);
        Task UpdateAsync(Food food);
        Task<bool> DeleteAsync(int id);
    }
}
