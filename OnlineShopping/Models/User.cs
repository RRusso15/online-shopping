namespace OnlineShopping.Models;

public abstract class User
{
    protected User(int id, string username, string password, UserRole role)
    {
        Id = id;
        Username = username;
        Password = password;
        Role = role;
    }

    public int Id { get; }
    public string Username { get; }
    public string Password { get; private set; }
    public UserRole Role { get; }

    public void UpdatePassword(string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            throw new ArgumentException("Password cannot be empty.", nameof(newPassword));
        }

        Password = newPassword;
    }
}
