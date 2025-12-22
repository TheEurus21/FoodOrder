using FoodOrder.Application.Interfaces;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FoodCategory>> GetAllAsync() =>
            await _context.Categories
                .Include(c => c.Foods)
                .Include(c => c.Restaurant)
                .ToListAsync();

        public async Task<FoodCategory?> GetByIdAsync(int id) =>
            await _context.Categories
                .Include(c => c.Foods)
                .Include(c => c.Restaurant)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<FoodCategory> AddAsync(FoodCategory category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(FoodCategory category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
