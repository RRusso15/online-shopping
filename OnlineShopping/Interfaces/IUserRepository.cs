using OnlineShopping.Models;

namespace OnlineShopping.Interfaces;

/// <summary>
/// Repository contract for user aggregate persistence operations.
/// </summary>
public interface IUserRepository
{
    int NextId();
    bool UsernameExists(string username);
    void Add(User user);
    User? FindByUsername(string username);
    User? FindByCredentials(string username, string password);
}
