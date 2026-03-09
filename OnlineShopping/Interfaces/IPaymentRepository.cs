using OnlineShopping.Models;

namespace OnlineShopping.Interfaces;

/// <summary>
/// Repository contract for payment persistence operations.
/// </summary>
public interface IPaymentRepository
{
    int NextId();
    void Add(Payment payment);
}
