using OnlineShopping.Interfaces;
using OnlineShopping.Models;
using OnlineShopping.Utilities;

namespace OnlineShopping.Menus;

public sealed class AdminMenu
{
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    private readonly IReportService _reportService;

    public AdminMenu(IProductService productService, IOrderService orderService, IReportService reportService)
    {
        _productService = productService;
        _orderService = orderService;
        _reportService = reportService;
    }

    public void Run(Administrator admin)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"=== ADMIN MENU ({admin.Username}) ===");
            Console.WriteLine("1. Add Product");
            Console.WriteLine("2. Update Product");
            Console.WriteLine("3. Delete Product");
            Console.WriteLine("4. Restock Product");
            Console.WriteLine("5. View Products");
            Console.WriteLine("6. View Orders");
            Console.WriteLine("7. Update Order Status");
            Console.WriteLine("8. View Low Stock Products");
            Console.WriteLine("9. Generate Sales Reports");
            Console.WriteLine("10. Logout");

            var choice = InputHelper.ReadInt("Choose an option: ", 1, 10);

            try
            {
                switch (choice)
                {
                    case 1:
                        AddProduct();
                        break;
                    case 2:
                        UpdateProduct();
                        break;
                    case 3:
                        DeleteProduct();
                        break;
                    case 4:
                        RestockProduct();
                        break;
                    case 5:
                        ViewProducts();
                        break;
                    case 6:
                        ViewOrders();
                        break;
                    case 7:
                        UpdateOrderStatus();
                        break;
                    case 8:
                        ViewLowStockProducts();
                        break;
                    case 9:
                        GenerateReport();
                        break;
                    case 10:
                        Console.WriteLine("Logged out.");
                        return;
                }
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException || ex is FormatException)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            InputHelper.Pause();
        }
    }

    private void AddProduct()
    {
        var name = InputHelper.ReadRequiredString("Name: ");
        var category = InputHelper.ReadRequiredString("Category: ");
        var description = InputHelper.ReadRequiredString("Description: ");
        var price = InputHelper.ReadDecimal("Price: ", 0.01m);
        var stock = InputHelper.ReadInt("Stock quantity: ", 0);

        var product = _productService.AddProduct(name, category, description, price, stock);
        Console.WriteLine($"Product added with ID #{product.Id}.");
    }

    private void UpdateProduct()
    {
        var id = InputHelper.ReadInt("Product ID: ", 1);
        var existing = _productService.GetProductById(id) ?? throw new KeyNotFoundException("Product not found.");

        Console.Write($"Name ({existing.Name}): ");
        var name = Console.ReadLine();
        Console.Write($"Category ({existing.Category}): ");
        var category = Console.ReadLine();
        Console.Write($"Description ({existing.Description}): ");
        var description = Console.ReadLine();
        var updatedPrice = InputHelper.ReadOptionalDecimal($"Price ({existing.Price}): ", 0.01m) ?? existing.Price;

        var updatedName = string.IsNullOrWhiteSpace(name) ? existing.Name : name.Trim();
        var updatedCategory = string.IsNullOrWhiteSpace(category) ? existing.Category : category.Trim();
        var updatedDescription = string.IsNullOrWhiteSpace(description) ? existing.Description : description.Trim();

        _productService.UpdateProduct(id, updatedName, updatedCategory, updatedDescription, updatedPrice);
        Console.WriteLine("Product updated.");
    }

    private void DeleteProduct()
    {
        var id = InputHelper.ReadInt("Product ID: ", 1);
        var deleted = _productService.DeleteProduct(id);
        Console.WriteLine(deleted ? "Product deleted." : "Product not found.");
    }

    private void RestockProduct()
    {
        var id = InputHelper.ReadInt("Product ID: ", 1);
        var qty = InputHelper.ReadInt("Quantity to add: ", 1);
        _productService.RestockProduct(id, qty);
        Console.WriteLine("Product restocked.");
    }

    private void ViewProducts()
    {
        var products = _productService.GetAllProducts().ToList();
        if (products.Count == 0)
        {
            Console.WriteLine("No products found.");
            return;
        }

        foreach (var p in products)
        {
            Console.WriteLine($"#{p.Id} | {p.Name} | {p.Category} | {p.Price:C} | Stock: {p.StockQuantity} | Rating: {p.AverageRating:F1}");
        }
    }

    private void ViewOrders()
    {
        var orders = _orderService.GetAllOrders().ToList();
        if (orders.Count == 0)
        {
            Console.WriteLine("No orders found.");
            return;
        }

        foreach (var order in orders)
        {
            Console.WriteLine($"Order #{order.Id} | Customer: {order.CustomerUsername} | {order.OrderDate:g} | {order.Status} | Total: {order.TotalAmount:C}");
        }
    }

    private void UpdateOrderStatus()
    {
        var orderId = InputHelper.ReadInt("Order ID: ", 1);
        Console.WriteLine("Order Status:");
        foreach (var status in Enum.GetValues<OrderStatus>())
        {
            Console.WriteLine($"{(int)status}. {status}");
        }

        var statusChoice = InputHelper.ReadInt("Choose new status: ", 1, Enum.GetValues<OrderStatus>().Length);
        var newStatus = (OrderStatus)statusChoice;

        _orderService.UpdateOrderStatus(orderId, newStatus);
        Console.WriteLine("Order status updated.");
    }

    private void ViewLowStockProducts()
    {
        var threshold = InputHelper.ReadInt("Low-stock threshold: ", 0);
        var products = _productService.GetLowStockProducts(threshold).ToList();

        if (products.Count == 0)
        {
            Console.WriteLine("No low-stock products found.");
            return;
        }

        foreach (var product in products)
        {
            Console.WriteLine($"#{product.Id} {product.Name} | Stock: {product.StockQuantity}");
        }
    }

    private void GenerateReport()
    {
        Console.WriteLine();
        Console.WriteLine(_reportService.GenerateSalesReport());
    }
}
