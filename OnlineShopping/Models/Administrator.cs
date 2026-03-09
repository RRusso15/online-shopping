namespace OnlineShopping.Models;

public sealed class Administrator : User
{
    public Administrator(int id, string username, string password)
        : base(id, username, password, UserRole.Administrator)
    {
    }
}
