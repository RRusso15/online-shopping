using System.Text.Json.Serialization;
using OnlineShopping.Interfaces;

namespace OnlineShopping.Models;

public sealed class Order
{
    public Order(int id, string customerUsername, List<OrderItem> items)
    {
        Id = id;
        CustomerUsername = customerUsername;
        Items = items;
        OrderDate = DateTime.Now;
        Status = OrderStatus.Pending;
        TotalAmount = items.Sum(i => i.Subtotal);
    }

    public int Id { get; }
    public string CustomerUsername { get; }
    public List<OrderItem> Items { get; }
    public decimal TotalAmount { get; }
    public DateTime OrderDate { get; }
    [JsonInclude]
    public OrderStatus Status { get; private set; }

    public void UpdateStatus(OrderStatus newStatus, IOrderStatusTransitionPolicy transitionPolicy)
    {
        transitionPolicy.EnsureCanTransition(Status, newStatus);
        Status = newStatus;
    }
}
