using OnlineShopping.Models;

namespace OnlineShopping.Interfaces;

public interface IAuthService
{
    bool Register(string username, string password, UserRole role, out string message);
    User? Login(string username, string password);
}
