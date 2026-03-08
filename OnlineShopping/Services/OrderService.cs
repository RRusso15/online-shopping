using OnlineShopping.Interfaces;
using OnlineShopping.Models;
using OnlineShopping.Utilities;

namespace OnlineShopping.Services;

public sealed class OrderService : IOrderService
{
    private readonly AppDataContext _context;
    private readonly IPaymentService _paymentService;

    public OrderService(AppDataContext context, IPaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    public (Order order, Payment payment) Checkout(Customer customer)
    {
        if (customer.Cart.Items.Count == 0)
        {
            throw new InvalidOperationException("Cart is empty.");
        }

        foreach (var cartItem in customer.Cart.Items)
        {
            if (cartItem.Quantity > cartItem.Product.StockQuantity)
            {
                throw new InvalidOperationException($"Insufficient stock for {cartItem.Product.Name}.");
            }
        }

        var orderItems = customer.Cart.Items
            .Select(i => new OrderItem(i.Product.Id, i.Product.Name, i.Product.Price, i.Quantity))
            .ToList();

        var order = new Order(_context.NextOrderId(), customer.Username, orderItems);
        _context.Orders.Add(order);

        try
        {
            var payment = _paymentService.ProcessPayment(customer, order);

            foreach (var cartItem in customer.Cart.Items)
            {
                cartItem.Product.StockQuantity -= cartItem.Quantity;
            }

            order.Status = OrderStatus.Paid;
            customer.Orders.Add(order);
            customer.Cart.Items.Clear();

            return (order, payment);
        }
        catch
        {
            _context.Orders.Remove(order);
            throw;
        }
    }

    public IEnumerable<Order> GetCustomerOrders(string username)
    {
        return _context.Orders
            .Where(o => o.CustomerUsername.Equals(username, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(o => o.OrderDate)
            .ToList();
    }

    public IEnumerable<Order> GetAllOrders()
    {
        return _context.Orders.OrderByDescending(o => o.OrderDate).ToList();
    }

    public Order? GetOrderById(int orderId)
    {
        return _context.Orders.FirstOrDefault(o => o.Id == orderId);
    }

    public void UpdateOrderStatus(int orderId, OrderStatus newStatus)
    {
        var order = GetOrderById(orderId) ?? throw new KeyNotFoundException("Order not found.");
        order.Status = newStatus;
    }
}
