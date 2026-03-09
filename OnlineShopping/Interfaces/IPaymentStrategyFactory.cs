using OnlineShopping.Models;

namespace OnlineShopping.Interfaces;

/// <summary>
/// Factory for resolving payment strategies by payment method.
/// </summary>
public interface IPaymentStrategyFactory
{
    IPaymentStrategy Create(PaymentMethod paymentMethod);
}
