namespace OnlineShopping.Models;

public sealed class Review
{
    public Review(int id, int productId, string customerUsername, int rating, string comment)
    {
        Id = id;
        ProductId = productId;
        CustomerUsername = customerUsername;
        Rating = rating;
        Comment = comment;
        CreatedAt = DateTime.Now;
    }

    public int Id { get; }
    public int ProductId { get; }
    public string CustomerUsername { get; }
    public int Rating { get; }
    public string Comment { get; }
    public DateTime CreatedAt { get; }
}
