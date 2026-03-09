using OnlineShopping.Models;

namespace OnlineShopping.Interfaces;

/// <summary>
/// Creates user entities from role and credentials.
/// </summary>
public interface IUserFactory
{
    User Create(int id, string username, string password, UserRole role);
}
