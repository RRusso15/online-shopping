using OnlineShopping.Models;

namespace OnlineShopping.Interfaces;

/// <summary>
/// Repository contract for order aggregate persistence operations.
/// </summary>
public interface IOrderRepository
{
    int NextId();
    IEnumerable<Order> GetAll();
    IEnumerable<Order> GetByCustomerUsername(string username);
    Order? GetById(int orderId);
    void Add(Order order);
    bool Remove(Order order);
}
