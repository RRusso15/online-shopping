using OnlineShopping.Models;

namespace OnlineShopping.Interfaces;

public interface ICartService
{
    Cart GetCart(Customer customer);
    void AddToCart(Customer customer, int productId, int quantity);
    void UpdateCartItem(Customer customer, int productId, int quantity);
    void RemoveFromCart(Customer customer, int productId);
    void ClearCart(Customer customer);
}
