using OnlineShopping.Interfaces;
using OnlineShopping.Models;

namespace OnlineShopping.Services;

public sealed class OrderStatusTransitionPolicy : IOrderStatusTransitionPolicy
{
    public bool CanTransition(OrderStatus current, OrderStatus next)
    {
        if (current == next)
        {
            return false;
        }

        return current switch
        {
            OrderStatus.Pending => next is OrderStatus.Paid or OrderStatus.Shipped or OrderStatus.Cancelled,
            OrderStatus.Paid => next is OrderStatus.Shipped or OrderStatus.Cancelled,
            OrderStatus.Shipped => next == OrderStatus.Delivered,
            OrderStatus.Delivered => false,
            OrderStatus.Cancelled => false,
            _ => false
        };
    }

    public void EnsureCanTransition(OrderStatus current, OrderStatus next)
    {
        if (!CanTransition(current, next))
        {
            throw new InvalidOperationException($"Cannot transition order status from {current} to {next}.");
        }
    }
}
