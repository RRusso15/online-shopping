using OnlineShopping.Interfaces;
using OnlineShopping.Models;

namespace OnlineShopping.Services;

/// <summary>
/// Payment strategy that records payment as pending for cash-on-delivery orders.
/// </summary>
public sealed class CashOnDeliveryPaymentStrategy : IPaymentStrategy
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IRepositorySession _repositorySession;

    public CashOnDeliveryPaymentStrategy(IPaymentRepository paymentRepository, IRepositorySession repositorySession)
    {
        _paymentRepository = paymentRepository;
        _repositorySession = repositorySession;
    }

    public PaymentMethod Method => PaymentMethod.CashOnDelivery;

    public Payment Process(Customer customer, Order order)
    {
        var pending = new Payment(
            _paymentRepository.NextId(),
            order.Id,
            customer.Username,
            order.TotalAmount,
            PaymentStatus.Pending,
            "Cash on delivery selected. Payment pending on delivery.");

        _paymentRepository.Add(pending);
        _repositorySession.SaveChanges();
        return pending;
    }
}
