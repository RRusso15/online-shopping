using OnlineShopping.Interfaces;
using OnlineShopping.Models;

namespace OnlineShopping.Services;

public sealed class WalletPaymentStrategy : IPaymentStrategy
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IRepositorySession _repositorySession;

    public WalletPaymentStrategy(IPaymentRepository paymentRepository, IRepositorySession repositorySession)
    {
        _paymentRepository = paymentRepository;
        _repositorySession = repositorySession;
    }

    public PaymentMethod Method => PaymentMethod.Wallet;

    public Payment Process(Customer customer, Order order)
    {
        if (customer.WalletBalance < order.TotalAmount)
        {
            var failed = new Payment(
                _paymentRepository.NextId(),
                order.Id,
                customer.Username,
                order.TotalAmount,
                PaymentStatus.Failed,
                "Insufficient wallet funds.");

            _paymentRepository.Add(failed);
            _repositorySession.SaveChanges();
            throw new InvalidOperationException("Insufficient wallet funds for checkout.");
        }

        customer.WalletBalance -= order.TotalAmount;
        var success = new Payment(
            _paymentRepository.NextId(),
            order.Id,
            customer.Username,
            order.TotalAmount,
            PaymentStatus.Success,
            "Payment successful.");

        _paymentRepository.Add(success);
        _repositorySession.SaveChanges();
        return success;
    }
}
