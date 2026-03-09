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
        var commands = BuildCommands(customer);

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== CUSTOMER MENU ===");
            foreach (var command in commands.OrderBy(c => c.Key))
            {
                Console.WriteLine($"{command.Key}. {command.Value.Label}");
            }

            var choice = InputHelper.ReadInt("Choose an option: ", 1, commands.Count);
            var result = ExecuteCommand(commands[choice]);
            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                Console.WriteLine(result.Message);
            }

            if (result.ShouldExit)
            {
                return;
            }

            InputHelper.Pause();
        }
    }

    private Dictionary<int, ICommand> BuildCommands(Customer customer)
    {
        return new Dictionary<int, ICommand>
        {
            [1] = new DelegateCommand("Browse Products", BrowseProducts),
            [2] = new DelegateCommand("Search Products", SearchProducts),
            [3] = new DelegateCommand("Add Product to Cart", () => AddProductToCart(customer)),
            [4] = new DelegateCommand("View Cart", () => ViewCart(customer)),
            [5] = new DelegateCommand("Update Cart", () => UpdateCart(customer)),
            [6] = new DelegateCommand("Checkout", () => Checkout(customer)),
            [7] = new DelegateCommand("View Wallet Balance", () => CommandResult.Ok($"Wallet Balance: {customer.WalletBalance:C}")),
            [8] = new DelegateCommand("Add Wallet Funds", () => AddWalletFunds(customer)),
            [9] = new DelegateCommand("View Order History", () => ViewOrderHistory(customer)),
            [10] = new DelegateCommand("Track Orders", () => TrackOrder(customer)),
            [11] = new DelegateCommand("Review Products", () => ReviewProducts(customer)),
            [12] = new DelegateCommand("Cancel Order", () => CancelOrder(customer)),
            [13] = new DelegateCommand("Logout", () => CommandResult.Exit("Logged out."))
        };
    }

    private static CommandResult ExecuteCommand(ICommand command)
    {
        try
        {
            return command.Execute();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException || ex is FormatException)
        {
            return CommandResult.Fail($"Error: {ex.Message}");
        }
    }

    private CommandResult BrowseProducts()
    {
        var products = _productService.GetAllProducts().ToList();
        if (products.Count == 0)
        {
            return CommandResult.Ok("No products available.");
        }

        var lines = new List<string>();
        foreach (var p in products)
        {
            lines.Add($"#{p.Id} | {p.Name} | {p.Category} | {p.Price:C} | Stock: {p.StockQuantity} | Rating: {p.AverageRating:F1} ({p.Reviews.Count})");
            lines.Add($"    {p.Description}");
        }

        return CommandResult.Ok(Environment.NewLine + string.Join(Environment.NewLine, lines));
    }

    private CommandResult SearchProducts()
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
            return CommandResult.Ok("No products match your search.");
        }

        var lines = new List<string>();
        foreach (var p in results)
        {
            lines.Add($"#{p.Id} | {p.Name} | {p.Category} | {p.Price:C} | Stock: {p.StockQuantity}");
        }

        return CommandResult.Ok(string.Join(Environment.NewLine, lines));
    }

    private CommandResult AddProductToCart(Customer customer)
    {
        var productId = InputHelper.ReadInt("Product ID: ", 1);
        var quantity = InputHelper.ReadInt("Quantity: ", 1);
        _cartService.AddToCart(customer, productId, quantity);
        return CommandResult.Ok("Item added to cart.");
    }

    private CommandResult ViewCart(Customer customer)
    {
        var cart = _cartService.GetCart(customer);
        if (cart.Items.Count == 0)
        {
            return CommandResult.Ok("Your cart is empty.");
        }

        var lines = new List<string> { "=== CART ===" };
        foreach (var item in cart.Items)
        {
            lines.Add($"#{item.Product.Id} {item.Product.Name} | {item.Quantity} x {item.Product.Price:C} = {item.Subtotal:C}");
        }

        lines.Add($"Total: {cart.Total:C}");
        return CommandResult.Ok(Environment.NewLine + string.Join(Environment.NewLine, lines));
    }

    private CommandResult UpdateCart(Customer customer)
    {
        var viewCartResult = ViewCart(customer);
        if (!string.IsNullOrWhiteSpace(viewCartResult.Message))
        {
            Console.WriteLine(viewCartResult.Message);
        }

        if (customer.Cart.Items.Count == 0)
        {
            return CommandResult.Ok();
        }

        Console.WriteLine("1. Update quantity");
        Console.WriteLine("2. Remove item");
        Console.WriteLine("3. Clear cart");
        var action = InputHelper.ReadInt("Choose action: ", 1, 3);

        if (action == 3)
        {
            _cartService.ClearCart(customer);
            return CommandResult.Ok("Cart cleared.");
        }

        var productId = InputHelper.ReadInt("Product ID: ", 1);

        if (action == 1)
        {
            var quantity = InputHelper.ReadInt("New quantity (0 removes item): ", 0);
            _cartService.UpdateCartItem(customer, productId, quantity);
            return CommandResult.Ok("Cart updated.");
        }

        _cartService.RemoveFromCart(customer, productId);
        return CommandResult.Ok("Item removed.");
    }

    private CommandResult Checkout(Customer customer)
    {
        Console.WriteLine("Payment Method:");
        Console.WriteLine("1. Wallet");
        Console.WriteLine("2. Cash on Delivery");
        var paymentChoice = InputHelper.ReadInt("Choose payment method: ", 1, 2);
        var paymentMethod = paymentChoice == 2 ? PaymentMethod.CashOnDelivery : PaymentMethod.Wallet;

        var (order, payment) = _orderService.Checkout(customer, paymentMethod);
        var lines = new[]
        {
            $"Order #{order.Id} placed successfully. Total: {order.TotalAmount:C}",
            $"Payment Status: {payment.Status} | Message: {payment.Message}",
            $"Remaining Wallet Balance: {customer.WalletBalance:C}",
            $"Current Order Status: {order.Status}"
        };

        return CommandResult.Ok(string.Join(Environment.NewLine, lines));
    }

    private CommandResult AddWalletFunds(Customer customer)
    {
        var amount = InputHelper.ReadDecimal("Amount to add: ", 0.01m);
        _paymentService.AddWalletFunds(customer, amount);
        return CommandResult.Ok($"Wallet updated. New balance: {customer.WalletBalance:C}");
    }

    private CommandResult ViewOrderHistory(Customer customer)
    {
        var orders = _orderService.GetCustomerOrders(customer.Username).ToList();
        if (orders.Count == 0)
        {
            return CommandResult.Ok("No orders found.");
        }

        var lines = new List<string>();
        foreach (var order in orders)
        {
            lines.Add(string.Empty);
            lines.Add($"Order #{order.Id} | {order.OrderDate:g} | {order.Status} | Total: {order.TotalAmount:C}");
            foreach (var item in order.Items)
            {
                lines.Add($"  - {item.ProductName}: {item.Quantity} x {item.UnitPrice:C} = {item.Subtotal:C}");
            }
        }

        return CommandResult.Ok(string.Join(Environment.NewLine, lines));
    }

    private CommandResult TrackOrder(Customer customer)
    {
        var orderId = InputHelper.ReadInt("Order ID: ", 1);
        var order = _orderService.GetOrderById(orderId);

        if (order is null || !order.CustomerUsername.Equals(customer.Username, StringComparison.OrdinalIgnoreCase))
        {
            return CommandResult.Fail("Order not found.");
        }

        return CommandResult.Ok($"Order #{order.Id} status: {order.Status}");
    }

    private CommandResult ReviewProducts(Customer customer)
    {
        var products = _productService.GetPurchasedProducts(customer).ToList();
        if (products.Count == 0)
        {
            return CommandResult.Ok("No purchased products available for review.");
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
        return CommandResult.Ok("Review submitted.");
    }

    private CommandResult CancelOrder(Customer customer)
    {
        var orderId = InputHelper.ReadInt("Order ID to cancel: ", 1);
        var refundPayment = _orderService.CancelOrder(customer, orderId);

        if (refundPayment.Status == PaymentStatus.Refunded)
        {
            return CommandResult.Ok($"Order cancelled and wallet refunded by {refundPayment.Amount:C}. New wallet balance: {customer.WalletBalance:C}");
        }

        return CommandResult.Ok("Order cancelled successfully.");
    }
}
