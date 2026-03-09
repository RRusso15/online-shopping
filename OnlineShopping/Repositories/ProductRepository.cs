using OnlineShopping.Interfaces;
using OnlineShopping.Models;
using OnlineShopping.Utilities;

namespace OnlineShopping.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly AppDataContext _context;

    public ProductRepository(AppDataContext context)
    {
        _context = context;
    }

    public int NextId() => _context.NextProductId();

    public int NextReviewId() => _context.NextReviewId();

    public IEnumerable<Product> GetAll() => _context.Products;

    public Product? GetById(int productId) => _context.Products.FirstOrDefault(p => p.Id == productId);

    public void Add(Product product)
    {
        _context.Products.Add(product);
    }

    public bool Remove(Product product)
    {
        return _context.Products.Remove(product);
    }
}
