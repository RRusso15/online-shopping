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
        var commands = BuildCommands();

        while (true)
        {
            Console.Clear();
            Console.WriteLine($"=== ADMIN MENU ({admin.Username}) ===");
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

    private Dictionary<int, ICommand> BuildCommands()
    {
        return new Dictionary<int, ICommand>
        {
            [1] = new DelegateCommand("Add Product", AddProduct),
            [2] = new DelegateCommand("Update Product", UpdateProduct),
            [3] = new DelegateCommand("Delete Product", DeleteProduct),
            [4] = new DelegateCommand("Restock Product", RestockProduct),
            [5] = new DelegateCommand("View Products", ViewProducts),
            [6] = new DelegateCommand("View Orders", ViewOrders),
            [7] = new DelegateCommand("Update Order Status", UpdateOrderStatus),
            [8] = new DelegateCommand("View Low Stock Products", ViewLowStockProducts),
            [9] = new DelegateCommand("Generate Sales Report", GenerateReport),
            [10] = new DelegateCommand("Export Sales Report CSV", ExportReportCsv),
            [11] = new DelegateCommand("Logout", () => CommandResult.Exit("Logged out."))
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

    private CommandResult AddProduct()
    {
        var name = InputHelper.ReadRequiredString("Name: ");
        var category = InputHelper.ReadRequiredString("Category: ");
        var description = InputHelper.ReadRequiredString("Description: ");
        var price = InputHelper.ReadDecimal("Price: ", 0.01m);
        var stock = InputHelper.ReadInt("Stock quantity: ", 0);

        var product = _productService.AddProduct(name, category, description, price, stock);
        return CommandResult.Ok($"Product added with ID #{product.Id}.");
    }

    private CommandResult UpdateProduct()
    {
        var id = InputHelper.ReadInt("Product ID: ", 1);
        var existing = _productService.GetProductById(id) ?? throw new KeyNotFoundException("Product not found.");

        Console.Write($"Name ({existing.Name}): ");
        var name = Console.ReadLine();
        Console.Write($"Category ({existing.Category}): ");
        var category = Console.ReadLine();
        Console.Write($"Description ({existing.Description}): ");
        var description = Console.ReadLine();
        var updatedPrice = InputHelper.ReadOptionalDecimal($"Price ({existing.Price}): ", 0.01m, decimal.MaxValue) ?? existing.Price;

        var updatedName = string.IsNullOrWhiteSpace(name) ? existing.Name : name.Trim();
        var updatedCategory = string.IsNullOrWhiteSpace(category) ? existing.Category : category.Trim();
        var updatedDescription = string.IsNullOrWhiteSpace(description) ? existing.Description : description.Trim();

        _productService.UpdateProduct(id, updatedName, updatedCategory, updatedDescription, updatedPrice);
        return CommandResult.Ok("Product updated.");
    }

    private CommandResult DeleteProduct()
    {
        var id = InputHelper.ReadInt("Product ID: ", 1);
        var deleted = _productService.DeleteProduct(id);
        return CommandResult.Ok(deleted ? "Product deleted." : "Product not found.");
    }

    private CommandResult RestockProduct()
    {
        var id = InputHelper.ReadInt("Product ID: ", 1);
        var qty = InputHelper.ReadInt("Quantity to add: ", 1);
        _productService.RestockProduct(id, qty);
        return CommandResult.Ok("Product restocked.");
    }

    private CommandResult ViewProducts()
    {
        var products = _productService.GetAllProducts().ToList();
        if (products.Count == 0)
        {
            return CommandResult.Ok("No products found.");
        }

        var lines = new List<string>();
        foreach (var p in products)
        {
            lines.Add($"#{p.Id} | {p.Name} | {p.Category} | {p.Price:C} | Stock: {p.StockQuantity} | Rating: {p.AverageRating:F1}");
        }

        return CommandResult.Ok(string.Join(Environment.NewLine, lines));
    }

    private CommandResult ViewOrders()
    {
        var orders = _orderService.GetAllOrders().ToList();
        if (orders.Count == 0)
        {
            return CommandResult.Ok("No orders found.");
        }

        var lines = new List<string>();
        foreach (var order in orders)
        {
            lines.Add($"Order #{order.Id} | Customer: {order.CustomerUsername} | {order.OrderDate:g} | {order.Status} | Total: {order.TotalAmount:C}");
        }

        return CommandResult.Ok(string.Join(Environment.NewLine, lines));
    }

    private CommandResult UpdateOrderStatus()
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
        return CommandResult.Ok("Order status updated.");
    }

    private CommandResult ViewLowStockProducts()
    {
        var threshold = InputHelper.ReadInt("Low-stock threshold: ", 0);
        var products = _productService.GetLowStockProducts(threshold).ToList();

        if (products.Count == 0)
        {
            return CommandResult.Ok("No low-stock products found.");
        }

        var lines = new List<string>();
        foreach (var product in products)
        {
            lines.Add($"#{product.Id} {product.Name} | Stock: {product.StockQuantity}");
        }

        return CommandResult.Ok(string.Join(Environment.NewLine, lines));
    }

    private CommandResult GenerateReport()
    {
        var (fromDate, toDate) = ReadDateRange();
        return CommandResult.Ok(_reportService.GenerateSalesReport(fromDate, toDate));
    }

    private CommandResult ExportReportCsv()
    {
        var outputDirectory = InputHelper.ReadRequiredString("Output directory (e.g. Reports): ");
        var (fromDate, toDate) = ReadDateRange();
        var filePath = _reportService.ExportSalesReportCsv(outputDirectory, fromDate, toDate);
        return CommandResult.Ok($"Report exported successfully: {filePath}");
    }

    private static (DateTime? fromDate, DateTime? toDate) ReadDateRange()
    {
        Console.WriteLine("Leave date blank to include all dates.");
        var fromDate = InputHelper.ReadOptionalDate("From date (yyyy-MM-dd): ");
        var toDate = InputHelper.ReadOptionalDate("To date (yyyy-MM-dd): ");

        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
        {
            throw new ArgumentException("From date cannot be after To date.");
        }

        return (fromDate, toDate);
    }
}
