using System.Text;
using OnlineShopping.Interfaces;
using OnlineShopping.Models;

namespace OnlineShopping.Services;

public sealed class ReportService : IReportService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public ReportService(IOrderRepository orderRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public string GenerateSalesReport(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var completedOrders = GetCompletedOrders(fromDate, toDate);

        var totalSales = completedOrders.Sum(o => o.TotalAmount);
        var orderCount = completedOrders.Count;

        var bestSellers = completedOrders
            .SelectMany(o => o.Items)
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                ProductName = g.First().ProductName,
                UnitsSold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Subtotal)
            })
            .OrderByDescending(x => x.UnitsSold)
            .ThenByDescending(x => x.Revenue)
            .Take(5)
            .ToList();

        var revenueByCategory = completedOrders
            .SelectMany(o => o.Items)
            .Join(
                _productRepository.GetAll(),
                item => item.ProductId,
                product => product.Id,
                (item, product) => new { product.Category, item.Subtotal })
            .GroupBy(x => x.Category)
            .Select(g => new { Category = g.Key, Revenue = g.Sum(x => x.Subtotal) })
            .OrderByDescending(x => x.Revenue)
            .ToList();

        var lowStockProducts = _productRepository.GetAll()
            .Where(p => p.StockQuantity <= 5)
            .OrderBy(p => p.StockQuantity)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("=== SALES REPORT ===");
        sb.AppendLine($"Generated On: {DateTime.Now:g}");
        if (fromDate.HasValue || toDate.HasValue)
        {
            sb.AppendLine($"Date Range: {fromDate?.ToString("yyyy-MM-dd") ?? "Any"} -> {toDate?.ToString("yyyy-MM-dd") ?? "Any"}");
        }
        sb.AppendLine($"Total Sales: {totalSales:C}");
        sb.AppendLine($"Number of Orders: {orderCount}");
        sb.AppendLine();

        sb.AppendLine("Top 5 Best-Selling Products:");
        if (bestSellers.Count == 0)
        {
            sb.AppendLine("- No sales data available.");
        }
        else
        {
            foreach (var item in bestSellers)
            {
                sb.AppendLine($"- #{item.ProductId} {item.ProductName}: {item.UnitsSold} units, {item.Revenue:C}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("Revenue by Category:");
        if (revenueByCategory.Count == 0)
        {
            sb.AppendLine("- No sales data available.");
        }
        else
        {
            foreach (var item in revenueByCategory)
            {
                sb.AppendLine($"- {item.Category}: {item.Revenue:C}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("Low Stock Products (<= 5):");
        if (lowStockProducts.Count == 0)
        {
            sb.AppendLine("- None");
        }
        else
        {
            foreach (var p in lowStockProducts)
            {
                sb.AppendLine($"- #{p.Id} {p.Name}: {p.StockQuantity} left");
            }
        }

        return sb.ToString();
    }

    public string ExportSalesReportCsv(string outputDirectory, DateTime? fromDate = null, DateTime? toDate = null)
    {
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentException("Output directory is required.", nameof(outputDirectory));
        }

        var completedOrders = GetCompletedOrders(fromDate, toDate);
        var lines = new List<string>
        {
            "OrderId,OrderDate,Customer,Status,TotalAmount"
        };

        foreach (var order in completedOrders)
        {
            lines.Add($"{order.Id},{order.OrderDate:yyyy-MM-dd HH:mm:ss},{Escape(order.CustomerUsername)},{order.Status},{order.TotalAmount:F2}");
        }

        Directory.CreateDirectory(outputDirectory);
        var filePath = Path.Combine(outputDirectory, $"sales-report-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
        File.WriteAllLines(filePath, lines);
        return filePath;
    }

    private List<Order> GetCompletedOrders(DateTime? fromDate, DateTime? toDate)
    {
        var completedStatuses = new[] { OrderStatus.Paid, OrderStatus.Shipped, OrderStatus.Delivered };
        return _orderRepository.GetAll()
            .Where(o => completedStatuses.Contains(o.Status))
            .Where(o => !fromDate.HasValue || o.OrderDate.Date >= fromDate.Value.Date)
            .Where(o => !toDate.HasValue || o.OrderDate.Date <= toDate.Value.Date)
            .ToList();
    }

    private static string Escape(string value)
    {
        return value.Contains(',') ? $"\"{value}\"" : value;
    }
}
