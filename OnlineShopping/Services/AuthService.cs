using OnlineShopping.Interfaces;
using OnlineShopping.Models;

namespace OnlineShopping.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserFactory _userFactory;
    private readonly IRepositorySession _repositorySession;

    public AuthService(IUserRepository userRepository, IUserFactory userFactory, IRepositorySession repositorySession)
    {
        _userRepository = userRepository;
        _userFactory = userFactory;
        _repositorySession = repositorySession;
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

        var exists = _userRepository.UsernameExists(username);
        if (exists)
        {
            message = "Username already exists.";
            return false;
        }

        var user = _userFactory.Create(_userRepository.NextId(), username, password, role);

        _userRepository.Add(user);
        _repositorySession.SaveChanges();
        message = "Registration successful.";
        return true;
    }

    public User? Login(string username, string password)
    {
        return _userRepository.FindByCredentials(username, password);
    }
}
