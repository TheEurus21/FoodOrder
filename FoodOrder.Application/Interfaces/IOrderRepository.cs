using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(int id);
        Task<Order> AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task<bool> DeleteAsync(int id);
        Task<int> CountTodayOrderPerRestaurant(int restaurantId);
        
    }
}
