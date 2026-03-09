using OnlineShopping.Interfaces;
using OnlineShopping.Models;
using OnlineShopping.Utilities;

namespace OnlineShopping.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDataContext _context;

    public UserRepository(AppDataContext context)
    {
        _context = context;
    }

    public int NextId() => _context.NextUserId();

    public bool UsernameExists(string username)
    {
        return _context.Users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public void Add(User user)
    {
        _context.Users.Add(user);
    }

    public User? FindByUsername(string username)
    {
        return _context.Users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public User? FindByCredentials(string username, string password)
    {
        return _context.Users.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Password == password);
    }
}
