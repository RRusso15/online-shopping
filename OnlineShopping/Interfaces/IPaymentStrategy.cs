using OnlineShopping.Models;

namespace OnlineShopping.Interfaces;

/// <summary>
/// Encapsulates a payment processing algorithm for a specific payment method.
/// </summary>
public interface IPaymentStrategy
{
    PaymentMethod Method { get; }
    Payment Process(Customer customer, Order order);
}
