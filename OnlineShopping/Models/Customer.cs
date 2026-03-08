namespace OnlineShopping.Models;

public sealed class Customer : User
{
    public Customer(int id, string username, string password)
        : base(id, username, password, UserRole.Customer)
    {
        Cart = new Cart(username);
        Orders = new List<Order>();
    }

    public decimal WalletBalance { get; set; }
    public Cart Cart { get; }
    public List<Order> Orders { get; }
}
