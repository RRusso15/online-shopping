using OnlineShopping.Interfaces;
using OnlineShopping.Models;

namespace OnlineShopping.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IRepositorySession _repositorySession;

    public ProductService(
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IRepositorySession repositorySession)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _repositorySession = repositorySession;
    }

    public IEnumerable<Product> GetAllProducts()
    {
        return _productRepository.GetAll().OrderBy(p => p.Id);
    }

    public IEnumerable<Product> SearchProducts(string? keyword, string? category, decimal? minPrice, decimal? maxPrice)
    {
        var query = _productRepository.GetAll();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(p =>
                p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        return query.OrderBy(p => p.Name).ToList();
    }

    public Product? GetProductById(int productId)
    {
        return _productRepository.GetById(productId);
    }

    public Product AddProduct(string name, string category, string description, decimal price, int stockQuantity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Category is required.", nameof(category));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required.", nameof(description));
        }

        if (price <= 0)
        {
            throw new ArgumentException("Price must be greater than zero.", nameof(price));
        }

        if (stockQuantity < 0)
        {
            throw new ArgumentException("Stock cannot be negative.", nameof(stockQuantity));
        }

        var product = new Product(_productRepository.NextId(), name.Trim(), category.Trim(), description.Trim(), price, stockQuantity);
        _productRepository.Add(product);
        _repositorySession.SaveChanges();
        return product;
    }

    public void UpdateProduct(int productId, string name, string category, string description, decimal price)
    {
        var product = GetProductById(productId) ?? throw new KeyNotFoundException("Product not found.");

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Category is required.", nameof(category));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required.", nameof(description));
        }

        if (price <= 0)
        {
            throw new ArgumentException("Price must be greater than zero.", nameof(price));
        }

        product.Name = name.Trim();
        product.Category = category.Trim();
        product.Description = description.Trim();
        product.Price = price;
        _repositorySession.SaveChanges();
    }

    public bool DeleteProduct(int productId)
    {
        var product = GetProductById(productId);
        if (product is null)
        {
            return false;
        }

        var removed = _productRepository.Remove(product);
        if (removed)
        {
            _repositorySession.SaveChanges();
        }

        return removed;
    }

    public void RestockProduct(int productId, int quantityToAdd)
    {
        if (quantityToAdd <= 0)
        {
            throw new ArgumentException("Restock quantity must be greater than zero.", nameof(quantityToAdd));
        }

        var product = GetProductById(productId) ?? throw new KeyNotFoundException("Product not found.");
        product.StockQuantity += quantityToAdd;
        _repositorySession.SaveChanges();
    }

    public void AddReview(int productId, Customer customer, int rating, string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            throw new ArgumentException("Review comment is required.", nameof(comment));
        }

        if (rating is < 1 or > 5)
        {
            throw new ArgumentException("Rating must be between 1 and 5.", nameof(rating));
        }

        var product = GetProductById(productId) ?? throw new KeyNotFoundException("Product not found.");

        var hasPurchased = _orderRepository.GetByCustomerUsername(customer.Username)
            .Where(o => o.CustomerUsername.Equals(customer.Username, StringComparison.OrdinalIgnoreCase)
                        && o.Status != OrderStatus.Cancelled)
            .SelectMany(o => o.Items)
            .Any(i => i.ProductId == productId);

        if (!hasPurchased)
        {
            throw new InvalidOperationException("You can only review products you have purchased.");
        }

        var alreadyReviewed = product.Reviews.Any(r =>
            r.CustomerUsername.Equals(customer.Username, StringComparison.OrdinalIgnoreCase));

        if (alreadyReviewed)
        {
            throw new InvalidOperationException("You have already reviewed this product.");
        }

        product.Reviews.Add(new Review(_productRepository.NextReviewId(), productId, customer.Username, rating, comment.Trim()));
        _repositorySession.SaveChanges();
    }

    public IEnumerable<Product> GetLowStockProducts(int threshold)
    {
        return _productRepository.GetAll()
            .Where(p => p.StockQuantity <= threshold)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.Name)
            .ToList();
    }

    public IEnumerable<Product> GetPurchasedProducts(Customer customer)
    {
        var purchasedIds = _orderRepository.GetByCustomerUsername(customer.Username)
            .Where(o => o.CustomerUsername.Equals(customer.Username, StringComparison.OrdinalIgnoreCase)
                        && o.Status != OrderStatus.Cancelled)
            .SelectMany(o => o.Items)
            .Select(i => i.ProductId)
            .Distinct()
            .ToHashSet();

        return _productRepository.GetAll().Where(p => purchasedIds.Contains(p.Id)).OrderBy(p => p.Name);
    }
}
