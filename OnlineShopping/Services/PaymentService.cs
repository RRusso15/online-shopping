using OnlineShopping.Interfaces;
using OnlineShopping.Models;

namespace OnlineShopping.Services;

public sealed class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IRepositorySession _repositorySession;

    public PaymentService(IPaymentRepository paymentRepository, IRepositorySession repositorySession)
    {
        _paymentRepository = paymentRepository;
        _repositorySession = repositorySession;
    }

    public void AddWalletFunds(Customer customer, decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
        }

        customer.WalletBalance += amount;
        _repositorySession.SaveChanges();
    }

    public Payment RefundToWallet(Customer customer, Order order, string reason)
    {
        customer.WalletBalance += order.TotalAmount;

        var refund = new Payment(
            _paymentRepository.NextId(),
            order.Id,
            customer.Username,
            order.TotalAmount,
            PaymentStatus.Refunded,
            reason);

        _paymentRepository.Add(refund);
        _repositorySession.SaveChanges();
        return refund;
    }
}
