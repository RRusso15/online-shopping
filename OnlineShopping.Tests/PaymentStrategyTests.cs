using OnlineShopping.Models;
using OnlineShopping.Repositories;
using OnlineShopping.Services;
using OnlineShopping.Utilities;

namespace OnlineShopping.Tests;

public sealed class PaymentStrategyTests
{
    [Fact]
    public void WalletPaymentStrategy_WithInsufficientFunds_ShouldThrowAndRecordFailedPayment()
    {
        var dbPath = BuildTempDatabasePath();
        try
        {
            var context = new AppDataContext(dbPath);
            var paymentRepository = new PaymentRepository(context);
            var session = new RepositorySession(context);
            var strategy = new WalletPaymentStrategy(paymentRepository, session);

            var customer = new Customer(1, "cust", "pass123") { WalletBalance = 10m };
            var order = new Order(1, customer.Username, [new OrderItem(1, "Item", 50m, 1)]);

            Assert.Throws<InvalidOperationException>(() => strategy.Process(customer, order));
            Assert.Single(context.Payments);
            Assert.Equal(PaymentStatus.Failed, context.Payments[0].Status);
        }
        finally
        {
            Cleanup(dbPath);
        }
    }

    [Fact]
    public void CashOnDeliveryStrategy_ShouldCreatePendingPayment()
    {
        var dbPath = BuildTempDatabasePath();
        try
        {
            var context = new AppDataContext(dbPath);
            var paymentRepository = new PaymentRepository(context);
            var session = new RepositorySession(context);
            var strategy = new CashOnDeliveryPaymentStrategy(paymentRepository, session);

            var customer = new Customer(1, "cust", "pass123");
            var order = new Order(1, customer.Username, [new OrderItem(1, "Item", 40m, 2)]);

            var payment = strategy.Process(customer, order);
            Assert.Equal(PaymentStatus.Pending, payment.Status);
            Assert.Equal(PaymentMethod.CashOnDelivery, strategy.Method);
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
