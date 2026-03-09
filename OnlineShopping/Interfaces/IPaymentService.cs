using OnlineShopping.Models;

namespace OnlineShopping.Interfaces;

public interface IPaymentService
{
    void AddWalletFunds(Customer customer, decimal amount);
    Payment ProcessPayment(Customer customer, Order order);
}
