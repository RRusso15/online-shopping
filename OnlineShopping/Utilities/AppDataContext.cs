using OnlineShopping.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OnlineShopping.Utilities;

public sealed class AppDataContext
{
    private readonly string _databasePath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.Preserve
    };

    public List<User> Users { get; } = new();
    public List<Product> Products { get; } = new();
    public List<Order> Orders { get; } = new();
    public List<Payment> Payments { get; } = new();

    private int _userId = 1;
    private int _productId = 1;
    private int _orderId = 1;
    private int _paymentId = 1;
    private int _reviewId = 1;

    public AppDataContext(string? databasePath = null)
    {
        _databasePath = databasePath ?? Path.Combine(Directory.GetCurrentDirectory(), "Data", "database.json");
        Load();
    }

    public int NextUserId() => _userId++;
    public int NextProductId() => _productId++;
    public int NextOrderId() => _orderId++;
    public int NextPaymentId() => _paymentId++;
    public int NextReviewId() => _reviewId++;

    public void SaveChanges()
    {
        var directory = Path.GetDirectoryName(_databasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var payload = new PersistedData
        {
            Users = Users,
            Products = Products,
            Orders = Orders,
            Payments = Payments
        };

        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        File.WriteAllText(_databasePath, json);
    }

    private void Load()
    {
        if (!File.Exists(_databasePath))
        {
            return;
        }

        var json = File.ReadAllText(_databasePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        var payload = JsonSerializer.Deserialize<PersistedData>(json, _jsonOptions);
        if (payload is null)
        {
            return;
        }

        Users.Clear();
        Products.Clear();
        Orders.Clear();
        Payments.Clear();

        Users.AddRange(payload.Users ?? []);
        Products.AddRange(payload.Products ?? []);
        Orders.AddRange(payload.Orders ?? []);
        Payments.AddRange(payload.Payments ?? []);

        RecalculateIdentityCounters();
    }

    private void RecalculateIdentityCounters()
    {
        _userId = Users.Count == 0 ? 1 : Users.Max(u => u.Id) + 1;
        _productId = Products.Count == 0 ? 1 : Products.Max(p => p.Id) + 1;
        _orderId = Orders.Count == 0 ? 1 : Orders.Max(o => o.Id) + 1;
        _paymentId = Payments.Count == 0 ? 1 : Payments.Max(p => p.Id) + 1;

        var maxReviewId = Products.SelectMany(p => p.Reviews).Select(r => r.Id).DefaultIfEmpty(0).Max();
        _reviewId = maxReviewId + 1;
    }

    private sealed class PersistedData
    {
        public List<User> Users { get; set; } = [];
        public List<Product> Products { get; set; } = [];
        public List<Order> Orders { get; set; } = [];
        public List<Payment> Payments { get; set; } = [];
    }
}
