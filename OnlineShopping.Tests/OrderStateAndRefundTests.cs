using OnlineShopping.Models;
using OnlineShopping.Repositories;
using OnlineShopping.Services;
using OnlineShopping.Utilities;

namespace OnlineShopping.Tests;

public sealed class OrderStateAndRefundTests
{
    [Fact]
    public void OrderStatusPolicy_ShouldRejectInvalidTransition()
    {
        var policy = new OrderStatusTransitionPolicy();

        Assert.False(policy.CanTransition(OrderStatus.Delivered, OrderStatus.Paid));
        Assert.Throws<InvalidOperationException>(() => policy.EnsureCanTransition(OrderStatus.Shipped, OrderStatus.Paid));
    }

    [Fact]
    public void CancelOrder_WhenPaid_ShouldRefundWallet()
    {
        var dbPath = BuildTempDatabasePath();
        try
        {
            var context = new AppDataContext(dbPath);
            var session = new RepositorySession(context);
            var userRepo = new UserRepository(context);
            var productRepo = new ProductRepository(context);
            var orderRepo = new OrderRepository(context);
            var paymentRepo = new PaymentRepository(context);

            var customer = new Customer(userRepo.NextId(), "cust", "pass123") { WalletBalance = 500m };
            userRepo.Add(customer);

            var product = new Product(productRepo.NextId(), "Laptop", "Electronics", "Test", 100m, 5);
            productRepo.Add(product);
            session.SaveChanges();

            var paymentService = new PaymentService(paymentRepo, session);
            var strategyFactory = new PaymentStrategyFactory([new WalletPaymentStrategy(paymentRepo, session)]);
            var orderService = new OrderService(orderRepo, userRepo, strategyFactory, paymentService, session, new OrderStatusTransitionPolicy());

            customer.Cart.Items.Add(new CartItem(product, 1));
            var (_, payment) = orderService.Checkout(customer, PaymentMethod.Wallet);
            Assert.Equal(PaymentStatus.Success, payment.Status);
            Assert.Equal(400m, customer.WalletBalance);

            var refund = orderService.CancelOrder(customer, 1);
            Assert.Equal(PaymentStatus.Refunded, refund.Status);
            Assert.Equal(500m, customer.WalletBalance);
        }
        finally
        {
            Cleanup(dbPath);
        }
    }

    private static string BuildTempDatabasePath()
    {
        return Path.Combine(Path.GetTempPath(), $"online-shopping-tests-{Guid.NewGuid():N}.json");
    }

    private static void Cleanup(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
