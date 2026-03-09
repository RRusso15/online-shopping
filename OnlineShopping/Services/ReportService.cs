using System.Text;
using OnlineShopping.Interfaces;
using OnlineShopping.Models;
using OnlineShopping.Utilities;

namespace OnlineShopping.Services;

public sealed class ReportService : IReportService
{
    private readonly AppDataContext _context;

    public ReportService(AppDataContext context)
    {
        _context = context;
    }

    public string GenerateSalesReport()
    {
        var completedStatuses = new[] { OrderStatus.Paid, OrderStatus.Shipped, OrderStatus.Delivered };
        var completedOrders = _context.Orders.Where(o => completedStatuses.Contains(o.Status)).ToList();

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
                _context.Products,
                item => item.ProductId,
                product => product.Id,
                (item, product) => new { product.Category, item.Subtotal })
            .GroupBy(x => x.Category)
            .Select(g => new { Category = g.Key, Revenue = g.Sum(x => x.Subtotal) })
            .OrderByDescending(x => x.Revenue)
            .ToList();

        var lowStockProducts = _context.Products
            .Where(p => p.StockQuantity <= 5)
            .OrderBy(p => p.StockQuantity)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("=== SALES REPORT ===");
        sb.AppendLine($"Generated On: {DateTime.Now:g}");
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
}
