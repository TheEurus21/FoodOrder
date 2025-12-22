using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces
{
    public interface IReviewRepository
    {
        Task<IEnumerable<Review>> GetAllAsync();
        Task<Review?> GetByIdAsync(int id);
        Task<Review> AddAsync(Review review);
        Task UpdateAsync(Review review);
        Task<bool> DeleteAsync(int id);
    }
}
