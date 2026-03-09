using OnlineShopping.Interfaces;
using OnlineShopping.Models;
using OnlineShopping.Utilities;

namespace OnlineShopping.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly AppDataContext _context;

    public OrderRepository(AppDataContext context)
    {
        _context = context;
    }

    public int NextId() => _context.NextOrderId();

    public IEnumerable<Order> GetAll() => _context.Orders;

    public IEnumerable<Order> GetByCustomerUsername(string username)
    {
        return _context.Orders.Where(o => o.CustomerUsername.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public Order? GetById(int orderId) => _context.Orders.FirstOrDefault(o => o.Id == orderId);

    public void Add(Order order)
    {
        _context.Orders.Add(order);
    }

    public bool Remove(Order order)
    {
        return _context.Orders.Remove(order);
    }
}
