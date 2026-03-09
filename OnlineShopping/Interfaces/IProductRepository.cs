using OnlineShopping.Models;

namespace OnlineShopping.Interfaces;

/// <summary>
/// Repository contract for product aggregate persistence operations.
/// </summary>
public interface IProductRepository
{
    int NextId();
    int NextReviewId();
    IEnumerable<Product> GetAll();
    Product? GetById(int productId);
    void Add(Product product);
    bool Remove(Product product);
}
