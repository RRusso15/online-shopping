using OnlineShopping.Interfaces;
using OnlineShopping.Models;
using OnlineShopping.Utilities;

namespace OnlineShopping.Services;

public sealed class PaymentService : IPaymentService
{
    private readonly AppDataContext _context;

    public PaymentService(AppDataContext context)
    {
        _context = context;
    }

    public void AddWalletFunds(Customer customer, decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
        }

        customer.WalletBalance += amount;
    }

    public Payment ProcessPayment(Customer customer, Order order)
    {
        if (customer.WalletBalance < order.TotalAmount)
        {
            var failed = new Payment(
                _context.NextPaymentId(),
                order.Id,
                customer.Username,
                order.TotalAmount,
                PaymentStatus.Failed,
                "Insufficient wallet funds.");

            _context.Payments.Add(failed);
            throw new InvalidOperationException("Insufficient wallet funds for checkout.");
        }

        customer.WalletBalance -= order.TotalAmount;
        var success = new Payment(
            _context.NextPaymentId(),
            order.Id,
            customer.Username,
            order.TotalAmount,
            PaymentStatus.Success,
            "Payment successful.");

        _context.Payments.Add(success);
        return success;
    }
}
