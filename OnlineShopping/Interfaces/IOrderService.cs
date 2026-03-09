using OnlineShopping.Models;

namespace OnlineShopping.Interfaces;

public interface IOrderService
{
    (Order order, Payment payment) Checkout(Customer customer);
    IEnumerable<Order> GetCustomerOrders(string username);
    IEnumerable<Order> GetAllOrders();
    Order? GetOrderById(int orderId);
    void UpdateOrderStatus(int orderId, OrderStatus newStatus);
}
