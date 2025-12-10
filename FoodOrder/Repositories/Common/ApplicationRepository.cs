using Microsoft.EntityFrameworkCore;
using FoodOrder.Data;
using FoodOrder.Models;
namespace FoodOrder.Repositories.Common
{
    public class ApplicationRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public ApplicationRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public async Task<List<T>>GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }
        public async Task<T>GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }
        public async Task<T>AddAsync(T entity)
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
            return entity;

        }
        public async Task<T>UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null) return false;
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<FoodCategory?> GetByOwnerIdAsync(int ownerId)
        {
            return await _context.Categories
                                 .FirstOrDefaultAsync(c => c.UserId == ownerId);
        }
        public async Task<Food?> GetFoodByNameAndOwnerAsync(string FoodName, int ownerId)
        {
            return await _context.Foods
                .Include(f => f.Category)
                .ThenInclude(c => c.Restaurant)
                .FirstOrDefaultAsync(f => f.Name == FoodName && f.Category.Restaurant.UserId == ownerId);
        }
        public async Task<Restaurant?> GetByNameAndOwnerAsync(string restaurantName, int ownerId)
        {
            return await _context.Restaurants
                .FirstOrDefaultAsync(r => r.UserId == ownerId && r.Name == restaurantName);
        }

        public async Task<Order?> GetLatestOrderByUserAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<Review?> GetLatestReviewByUserAsync(int userId)
        {
            return await _context.Reviews
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();
        }
        public async Task<Review?> GetByRestaurantNameAndUserAsync(string restaurantName, int userId)
        {
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Name == restaurantName && r.UserId == userId);

            if (restaurant == null) return null;

            return await _context.Reviews
                .FirstOrDefaultAsync(rv => rv.UserId == userId && rv.RestaurantId == restaurant.Id);
        }


    }

}
