using OnlineShopping.Interfaces;
using OnlineShopping.Models;

namespace OnlineShopping.Services;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPaymentStrategyFactory _paymentStrategyFactory;
    private readonly IPaymentService _paymentService;
    private readonly IRepositorySession _repositorySession;
    private readonly IOrderStatusTransitionPolicy _statusTransitionPolicy;

    public OrderService(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IPaymentStrategyFactory paymentStrategyFactory,
        IPaymentService paymentService,
        IRepositorySession repositorySession,
        IOrderStatusTransitionPolicy statusTransitionPolicy)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _paymentStrategyFactory = paymentStrategyFactory;
        _paymentService = paymentService;
        _repositorySession = repositorySession;
        _statusTransitionPolicy = statusTransitionPolicy;
    }

    public (Order order, Payment payment) Checkout(Customer customer, PaymentMethod paymentMethod = PaymentMethod.Wallet)
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

        var order = new Order(_orderRepository.NextId(), customer.Username, orderItems);
        _orderRepository.Add(order);

        try
        {
            var paymentStrategy = _paymentStrategyFactory.Create(paymentMethod);
            var payment = paymentStrategy.Process(customer, order);

            foreach (var cartItem in customer.Cart.Items)
            {
                cartItem.Product.StockQuantity -= cartItem.Quantity;
            }

            if (payment.Status == PaymentStatus.Success)
            {
                order.UpdateStatus(OrderStatus.Paid, _statusTransitionPolicy);
            }

            customer.Orders.Add(order);
            customer.Cart.Items.Clear();
            _repositorySession.SaveChanges();

            return (order, payment);
        }
        catch
        {
            _orderRepository.Remove(order);
            _repositorySession.SaveChanges();
            throw;
        }
    }

    public Payment CancelOrder(Customer customer, int orderId)
    {
        var order = GetOrderById(orderId) ?? throw new KeyNotFoundException("Order not found.");
        if (!order.CustomerUsername.Equals(customer.Username, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("You can only cancel your own orders.");
        }

        var wasPaid = order.Status == OrderStatus.Paid;
        order.UpdateStatus(OrderStatus.Cancelled, _statusTransitionPolicy);
        _repositorySession.SaveChanges();

        if (wasPaid)
        {
            return _paymentService.RefundToWallet(customer, order, "Order cancelled. Wallet refunded.");
        }

        return new Payment(0, order.Id, customer.Username, 0m, PaymentStatus.Pending, "Order cancelled. No wallet refund required.");
    }

    public IEnumerable<Order> GetCustomerOrders(string username)
    {
        return _orderRepository.GetByCustomerUsername(username)
            .Where(o => o.CustomerUsername.Equals(username, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(o => o.OrderDate)
            .ToList();
    }

    public IEnumerable<Order> GetAllOrders()
    {
        return _orderRepository.GetAll().OrderByDescending(o => o.OrderDate).ToList();
    }

    public Order? GetOrderById(int orderId)
    {
        return _orderRepository.GetById(orderId);
    }

    public void UpdateOrderStatus(int orderId, OrderStatus newStatus)
    {
        var order = GetOrderById(orderId) ?? throw new KeyNotFoundException("Order not found.");
        var wasPaid = order.Status == OrderStatus.Paid;
        order.UpdateStatus(newStatus, _statusTransitionPolicy);
        _repositorySession.SaveChanges();

        if (newStatus == OrderStatus.Cancelled && wasPaid)
        {
            var customer = FindCustomerByUsername(order.CustomerUsername);
            if (customer is not null)
            {
                _paymentService.RefundToWallet(customer, order, "Admin cancelled paid order. Wallet refunded.");
            }
        }
    }

    private Customer? FindCustomerByUsername(string username)
    {
        return _userRepository.FindByUsername(username) as Customer;
    }
}
