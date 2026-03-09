using OnlineShopping.Models;

namespace OnlineShopping.Interfaces;

/// <summary>
/// Defines and validates legal order-status transitions.
/// </summary>
public interface IOrderStatusTransitionPolicy
{
    bool CanTransition(OrderStatus current, OrderStatus next);
    void EnsureCanTransition(OrderStatus current, OrderStatus next);
}
