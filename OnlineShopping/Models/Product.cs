namespace OnlineShopping.Models;

public sealed class Product
{
    public Product(int id, string name, string category, string description, decimal price, int stockQuantity)
    {
        Id = id;
        Name = name;
        Category = category;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        Reviews = new List<Review>();
    }

    public int Id { get; }
    public string Name { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public List<Review> Reviews { get; }
    public double AverageRating => Reviews.Count == 0 ? 0 : Reviews.Average(r => r.Rating);
}
