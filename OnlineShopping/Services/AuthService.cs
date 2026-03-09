using OnlineShopping.Interfaces;
using OnlineShopping.Models;
using OnlineShopping.Utilities;

namespace OnlineShopping.Services;

public sealed class AuthService : IAuthService
{
    private readonly AppDataContext _context;

    public AuthService(AppDataContext context)
    {
        _context = context;
    }

    public bool Register(string username, string password, UserRole role, out string message)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            message = "Username and password are required.";
            return false;
        }

        username = username.Trim();
        if (password.Length < 6)
        {
            message = "Password must be at least 6 characters.";
            return false;
        }

        var exists = _context.Users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        if (exists)
        {
            message = "Username already exists.";
            return false;
        }

        User user = role switch
        {
            UserRole.Administrator => new Administrator(_context.NextUserId(), username, password),
            _ => new Customer(_context.NextUserId(), username, password)
        };

        _context.Users.Add(user);
        _context.SaveChanges();
        message = "Registration successful.";
        return true;
    }

    public User? Login(string username, string password)
    {
        return _context.Users.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Password == password);
    }
}
