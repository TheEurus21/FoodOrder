using FoodOrder.Application.Interfaces;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories
{
    public class FoodRepository : IFoodRepository
    {
        private readonly AppDbContext _context;

        public FoodRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Food>> GetAllAsync() =>
            await _context.Foods
                .Include(f => f.Category)
                .ThenInclude(c => c.Restaurant)
                .ToListAsync();

        public async Task<Food?> GetByIdAsync(int id) =>
            await _context.Foods
                .Include(f => f.Category)
                .ThenInclude(c => c.Restaurant)
                .FirstOrDefaultAsync(f => f.Id == id);

        public async Task<Food> AddAsync(Food food)
        {
            _context.Foods.Add(food);
            await _context.SaveChangesAsync();
            return food;
        }

        public async Task UpdateAsync(Food food)
        {
            _context.Foods.Update(food);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null) return false;

            _context.Foods.Remove(food);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
