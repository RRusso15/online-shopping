using OnlineShopping.Interfaces;
using OnlineShopping.Models;

namespace OnlineShopping.Services;

/// <summary>
/// Factory for creating concrete user instances by role.
/// </summary>
public sealed class UserFactory : IUserFactory
{
    public User Create(int id, string username, string password, UserRole role)
    {
        return role switch
        {
            UserRole.Administrator => new Administrator(id, username, password),
            UserRole.Customer => new Customer(id, username, password),
            _ => throw new NotSupportedException($"Unsupported role '{role}'.")
        };
    }
}
