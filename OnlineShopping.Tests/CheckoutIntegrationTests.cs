using OnlineShopping.Models;
using OnlineShopping.Repositories;
using OnlineShopping.Services;
using OnlineShopping.Utilities;

namespace OnlineShopping.Tests;

public sealed class CheckoutIntegrationTests
{
    [Fact]
    public void Checkout_WithWalletPayment_ShouldCreatePaidOrderAndClearCart()
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

            var customer = new Customer(userRepo.NextId(), "cust", "pass123") { WalletBalance = 1000m };
            userRepo.Add(customer);

            var product = new Product(productRepo.NextId(), "Phone", "Electronics", "Demo product", 300m, 10);
            productRepo.Add(product);
            session.SaveChanges();

            var paymentService = new PaymentService(paymentRepo, session);
            var strategyFactory = new PaymentStrategyFactory([new WalletPaymentStrategy(paymentRepo, session), new CashOnDeliveryPaymentStrategy(paymentRepo, session)]);
            var orderService = new OrderService(orderRepo, userRepo, strategyFactory, paymentService, session, new OrderStatusTransitionPolicy());

            customer.Cart.Items.Add(new CartItem(product, 2));
            var (order, payment) = orderService.Checkout(customer, PaymentMethod.Wallet);

            Assert.Equal(PaymentStatus.Success, payment.Status);
            Assert.Equal(OrderStatus.Paid, order.Status);
            Assert.Empty(customer.Cart.Items);
            Assert.Equal(400m, customer.WalletBalance);
            Assert.Equal(8, product.StockQuantity);
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
