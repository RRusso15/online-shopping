using OnlineShopping.Interfaces;
using OnlineShopping.Models;
using OnlineShopping.Utilities;

namespace OnlineShopping.Menus;

public sealed class CustomerMenu
{
    private readonly IProductService _productService;
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;

    public CustomerMenu(
        IProductService productService,
        ICartService cartService,
        IOrderService orderService,
        IPaymentService paymentService)
    {
        _productService = productService;
        _cartService = cartService;
        _orderService = orderService;
        _paymentService = paymentService;
    }

    public void Run(Customer customer)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== CUSTOMER MENU ===");
            Console.WriteLine("1. Browse Products");
            Console.WriteLine("2. Search Products");
            Console.WriteLine("3. Add Product to Cart");
            Console.WriteLine("4. View Cart");
            Console.WriteLine("5. Update Cart");
            Console.WriteLine("6. Checkout");
            Console.WriteLine("7. View Wallet Balance");
            Console.WriteLine("8. Add Wallet Funds");
            Console.WriteLine("9. View Order History");
            Console.WriteLine("10. Track Orders");
            Console.WriteLine("11. Review Products");
            Console.WriteLine("12. Logout");

            var choice = InputHelper.ReadInt("Choose an option: ", 1, 12);

            try
            {
                switch (choice)
                {
                    case 1:
                        BrowseProducts();
                        break;
                    case 2:
                        SearchProducts();
                        break;
                    case 3:
                        AddProductToCart(customer);
                        break;
                    case 4:
                        ViewCart(customer);
                        break;
                    case 5:
                        UpdateCart(customer);
                        break;
                    case 6:
                        Checkout(customer);
                        break;
                    case 7:
                        Console.WriteLine($"Wallet Balance: {customer.WalletBalance:C}");
                        break;
                    case 8:
                        AddWalletFunds(customer);
                        break;
                    case 9:
                        ViewOrderHistory(customer);
                        break;
                    case 10:
                        TrackOrder(customer);
                        break;
                    case 11:
                        ReviewProducts(customer);
                        break;
                    case 12:
                        Console.WriteLine("Logged out.");
                        return;
                }
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException || ex is FormatException)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private void BrowseProducts()
    {
        var products = _productService.GetAllProducts().ToList();
        if (products.Count == 0)
        {
            Console.WriteLine("No products available.");
            return;
        }

        Console.WriteLine();
        foreach (var p in products)
        {
            Console.WriteLine($"#{p.Id} | {p.Name} | {p.Category} | {p.Price:C} | Stock: {p.StockQuantity} | Rating: {p.AverageRating:F1} ({p.Reviews.Count})");
            Console.WriteLine($"    {p.Description}");
        }
    }

    private void SearchProducts()
    {
        Console.WriteLine();
        Console.WriteLine("Leave any field blank to skip that filter.");
        Console.Write("Keyword (name/description): ");
        var keyword = Console.ReadLine();
        Console.Write("Category: ");
        var category = Console.ReadLine();
        var min = InputHelper.ReadOptionalDecimal("Minimum price: ");
        var max = InputHelper.ReadOptionalDecimal("Maximum price: ");

        if (min.HasValue && max.HasValue && min > max)
        {
            throw new ArgumentException("Minimum price cannot be greater than maximum price.");
        }

        var results = _productService.SearchProducts(keyword, category, min, max).ToList();
        if (results.Count == 0)
        {
            Console.WriteLine("No products match your search.");
            return;
        }

        foreach (var p in results)
        {
            Console.WriteLine($"#{p.Id} | {p.Name} | {p.Category} | {p.Price:C} | Stock: {p.StockQuantity}");
        }
    }

    private void AddProductToCart(Customer customer)
    {
        var productId = InputHelper.ReadInt("Product ID: ", 1);
        var quantity = InputHelper.ReadInt("Quantity: ", 1);
        _cartService.AddToCart(customer, productId, quantity);
        Console.WriteLine("Item added to cart.");
    }

    private void ViewCart(Customer customer)
    {
        var cart = _cartService.GetCart(customer);
        if (cart.Items.Count == 0)
        {
            Console.WriteLine("Your cart is empty.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("=== CART ===");
        foreach (var item in cart.Items)
        {
            Console.WriteLine($"#{item.Product.Id} {item.Product.Name} | {item.Quantity} x {item.Product.Price:C} = {item.Subtotal:C}");
        }

        Console.WriteLine($"Total: {cart.Total:C}");
    }

    private void UpdateCart(Customer customer)
    {
        ViewCart(customer);
        if (customer.Cart.Items.Count == 0)
        {
            return;
        }

        Console.WriteLine("1. Update quantity");
        Console.WriteLine("2. Remove item");
        Console.WriteLine("3. Clear cart");
        var action = InputHelper.ReadInt("Choose action: ", 1, 3);

        if (action == 3)
        {
            _cartService.ClearCart(customer);
            Console.WriteLine("Cart cleared.");
            return;
        }

        var productId = InputHelper.ReadInt("Product ID: ", 1);

        if (action == 1)
        {
            var quantity = InputHelper.ReadInt("New quantity (0 removes item): ", 0);
            _cartService.UpdateCartItem(customer, productId, quantity);
            Console.WriteLine("Cart updated.");
        }
        else
        {
            _cartService.RemoveFromCart(customer, productId);
            Console.WriteLine("Item removed.");
        }
    }

    private void Checkout(Customer customer)
    {
        var (order, payment) = _orderService.Checkout(customer);
        Console.WriteLine($"Order #{order.Id} placed successfully. Total: {order.TotalAmount:C}");
        Console.WriteLine($"Payment Status: {payment.Status} | Message: {payment.Message}");
        Console.WriteLine($"Remaining Wallet Balance: {customer.WalletBalance:C}");
    }

    private void AddWalletFunds(Customer customer)
    {
        var amount = InputHelper.ReadDecimal("Amount to add: ", 0.01m);
        _paymentService.AddWalletFunds(customer, amount);
        Console.WriteLine($"Wallet updated. New balance: {customer.WalletBalance:C}");
    }

    private void ViewOrderHistory(Customer customer)
    {
        var orders = _orderService.GetCustomerOrders(customer.Username).ToList();
        if (orders.Count == 0)
        {
            Console.WriteLine("No orders found.");
            return;
        }

        foreach (var order in orders)
        {
            Console.WriteLine();
            Console.WriteLine($"Order #{order.Id} | {order.OrderDate:g} | {order.Status} | Total: {order.TotalAmount:C}");
            foreach (var item in order.Items)
            {
                Console.WriteLine($"  - {item.ProductName}: {item.Quantity} x {item.UnitPrice:C} = {item.Subtotal:C}");
            }
        }
    }

    private void TrackOrder(Customer customer)
    {
        var orderId = InputHelper.ReadInt("Order ID: ", 1);
        var order = _orderService.GetOrderById(orderId);

        if (order is null || !order.CustomerUsername.Equals(customer.Username, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Order not found.");
            return;
        }

        Console.WriteLine($"Order #{order.Id} status: {order.Status}");
    }

    private void ReviewProducts(Customer customer)
    {
        var products = _productService.GetPurchasedProducts(customer).ToList();
        if (products.Count == 0)
        {
            Console.WriteLine("No purchased products available for review.");
            return;
        }

        Console.WriteLine("Purchased Products:");
        foreach (var p in products)
        {
            Console.WriteLine($"#{p.Id} {p.Name} | Current Rating: {p.AverageRating:F1}");
        }

        var productId = InputHelper.ReadInt("Product ID to review: ", 1);
        var rating = InputHelper.ReadInt("Rating (1-5): ", 1, 5);
        var comment = InputHelper.ReadRequiredString("Comment: ");

        _productService.AddReview(productId, customer, rating, comment);
        Console.WriteLine("Review submitted.");
    }
}
