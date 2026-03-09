using OnlineShopping.Interfaces;
using OnlineShopping.Models;
using OnlineShopping.Utilities;

namespace OnlineShopping.Services;

public sealed class CartService : ICartService
{
    private readonly IProductService _productService;
    private readonly AppDataContext _context;

    public CartService(IProductService productService, AppDataContext context)
    {
        _productService = productService;
        _context = context;
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
            _context.SaveChanges();
            return;
        }

        if (existingItem.Quantity + quantity > product.StockQuantity)
        {
            throw new InvalidOperationException("Requested quantity exceeds available stock.");
        }

        existingItem.Quantity += quantity;
        _context.SaveChanges();
    }

    public void UpdateCartItem(Customer customer, int productId, int quantity)
    {
        var item = customer.Cart.Items.FirstOrDefault(i => i.Product.Id == productId)
                   ?? throw new KeyNotFoundException("Product not found in cart.");

        if (quantity <= 0)
        {
            customer.Cart.Items.Remove(item);
            _context.SaveChanges();
            return;
        }

        if (quantity > item.Product.StockQuantity)
        {
            throw new InvalidOperationException("Requested quantity exceeds available stock.");
        }

        item.Quantity = quantity;
        _context.SaveChanges();
    }

    public void RemoveFromCart(Customer customer, int productId)
    {
        var item = customer.Cart.Items.FirstOrDefault(i => i.Product.Id == productId)
                   ?? throw new KeyNotFoundException("Product not found in cart.");

        customer.Cart.Items.Remove(item);
        _context.SaveChanges();
    }

    public void ClearCart(Customer customer)
    {
        customer.Cart.Items.Clear();
        _context.SaveChanges();
    }
}
