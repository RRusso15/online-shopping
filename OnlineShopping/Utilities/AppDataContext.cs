using OnlineShopping.Models;

namespace OnlineShopping.Utilities;

public sealed class AppDataContext
{
    public List<User> Users { get; } = new();
    public List<Product> Products { get; } = new();
    public List<Order> Orders { get; } = new();
    public List<Payment> Payments { get; } = new();

    private int _userId = 1;
    private int _productId = 1;
    private int _orderId = 1;
    private int _paymentId = 1;
    private int _reviewId = 1;

    public int NextUserId() => _userId++;
    public int NextProductId() => _productId++;
    public int NextOrderId() => _orderId++;
    public int NextPaymentId() => _paymentId++;
    public int NextReviewId() => _reviewId++;
}
