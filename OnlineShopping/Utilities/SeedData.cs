using OnlineShopping.Models;

namespace OnlineShopping.Utilities;

public static class SeedData
{
    public static void Initialize(AppDataContext context)
    {
        if (context.Users.Count > 0 || context.Products.Count > 0)
        {
            return;
        }

        var admin = new Administrator(context.NextUserId(), "admin", "admin123");
        var customer = new Customer(context.NextUserId(), "customer", "cust123")
        {
            WalletBalance = 5000m
        };

        context.Users.Add(admin);
        context.Users.Add(customer);

        context.Products.AddRange(new[]
        {
            new Product(context.NextProductId(), "Laptop Pro 14", "Electronics", "14-inch productivity laptop", 1999.99m, 10),
            new Product(context.NextProductId(), "Wireless Headphones", "Electronics", "Noise-cancelling over-ear headphones", 249.50m, 25),
            new Product(context.NextProductId(), "Office Chair", "Furniture", "Ergonomic chair with lumbar support", 179.99m, 7),
            new Product(context.NextProductId(), "Running Shoes", "Sports", "Lightweight training shoes", 89.99m, 30),
            new Product(context.NextProductId(), "Coffee Maker", "Home", "Automatic drip coffee machine", 59.99m, 5)
        });

        context.SaveChanges();
    }
}
