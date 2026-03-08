using OnlineShopping.Interfaces;
using OnlineShopping.Models;

namespace OnlineShopping.Services;

public sealed class CartService : ICartService
{
    private readonly IProductService _productService;

    public CartService(IProductService productService)
    {
        _productService = productService;
    }

    public Cart GetCart(Customer customer)
    {
        return customer.Cart;
    }

    public void AddToCart(Customer customer, int productId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        var product = _productService.GetProductById(productId) ?? throw new KeyNotFoundException("Product not found.");
        if (product.StockQuantity < quantity)
        {
            throw new InvalidOperationException("Insufficient stock.");
        }

        var existingItem = customer.Cart.Items.FirstOrDefault(i => i.Product.Id == productId);
        if (existingItem is null)
        {
            customer.Cart.Items.Add(new CartItem(product, quantity));
            return;
        }

        if (existingItem.Quantity + quantity > product.StockQuantity)
        {
            throw new InvalidOperationException("Requested quantity exceeds available stock.");
        }

        existingItem.Quantity += quantity;
    }

    public void UpdateCartItem(Customer customer, int productId, int quantity)
    {
        var item = customer.Cart.Items.FirstOrDefault(i => i.Product.Id == productId)
                   ?? throw new KeyNotFoundException("Product not found in cart.");

        if (quantity <= 0)
        {
            customer.Cart.Items.Remove(item);
            return;
        }

        if (quantity > item.Product.StockQuantity)
        {
            throw new InvalidOperationException("Requested quantity exceeds available stock.");
        }

        item.Quantity = quantity;
    }

    public void RemoveFromCart(Customer customer, int productId)
    {
        var item = customer.Cart.Items.FirstOrDefault(i => i.Product.Id == productId)
                   ?? throw new KeyNotFoundException("Product not found in cart.");

        customer.Cart.Items.Remove(item);
    }

    public void ClearCart(Customer customer)
    {
        customer.Cart.Items.Clear();
    }
}
