using OnlineShopping.Models;

namespace OnlineShopping.Interfaces;

public interface IOrderService
{
    (Order order, Payment payment) Checkout(Customer customer, PaymentMethod paymentMethod = PaymentMethod.Wallet);
    Payment CancelOrder(Customer customer, int orderId);
    IEnumerable<Order> GetCustomerOrders(string username);
    IEnumerable<Order> GetAllOrders();
    Order? GetOrderById(int orderId);
    void UpdateOrderStatus(int orderId, OrderStatus newStatus);
}
