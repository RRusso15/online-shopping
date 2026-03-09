using OnlineShopping.Models;

namespace OnlineShopping.Interfaces;

public interface IPaymentService
{
    void AddWalletFunds(Customer customer, decimal amount);
    Payment RefundToWallet(Customer customer, Order order, string reason);
}
