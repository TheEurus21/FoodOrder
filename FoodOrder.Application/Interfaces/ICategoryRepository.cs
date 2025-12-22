using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<FoodCategory>> GetAllAsync();
        Task<FoodCategory?> GetByIdAsync(int id);
        Task<FoodCategory> AddAsync(FoodCategory category);
        Task UpdateAsync(FoodCategory category);
        Task<bool> DeleteAsync(int id);
    }
}
