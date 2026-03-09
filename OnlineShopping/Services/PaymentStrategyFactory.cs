using OnlineShopping.Interfaces;
using OnlineShopping.Models;

namespace OnlineShopping.Services;

/// <summary>
/// Factory that returns the configured strategy for a payment method.
/// </summary>
public sealed class PaymentStrategyFactory : IPaymentStrategyFactory
{
    private readonly Dictionary<PaymentMethod, IPaymentStrategy> _strategies;

    public PaymentStrategyFactory(IEnumerable<IPaymentStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.Method);
    }

    public IPaymentStrategy Create(PaymentMethod paymentMethod)
    {
        if (_strategies.TryGetValue(paymentMethod, out var strategy))
        {
            return strategy;
        }

        throw new NotSupportedException($"Payment method '{paymentMethod}' is not supported.");
    }
}
