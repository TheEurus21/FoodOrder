using FoodOrder.Data;
using FoodOrder.Models;
using FoodOrder.Repositories.Common;

public class UserRepository : ApplicationRepository<User>
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public User FindByUsernameAndPassword(string username, string password)
    {
        return _context.Users
            .FirstOrDefault(u => u.Username == username && u.Password == password);
    }
}
