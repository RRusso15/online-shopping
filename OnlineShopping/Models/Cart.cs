namespace OnlineShopping.Models;

public sealed class Cart
{
    public Cart(string customerUsername)
    {
        CustomerUsername = customerUsername;
        Items = new List<CartItem>();
    }

    public string CustomerUsername { get; }
    public List<CartItem> Items { get; }
    public decimal Total => Items.Sum(i => i.Subtotal);
}
