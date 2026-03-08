namespace OnlineShopping.Models;

public sealed class CartItem
{
    public CartItem(Product product, int quantity)
    {
        Product = product;
        Quantity = quantity;
    }

    public Product Product { get; }
    public int Quantity { get; set; }
    public decimal Subtotal => Product.Price * Quantity;
}
