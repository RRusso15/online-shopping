using OnlineShopping.Models;

namespace OnlineShopping.Interfaces;

public interface IProductService
{
    IEnumerable<Product> GetAllProducts();
    IEnumerable<Product> SearchProducts(string? keyword, string? category, decimal? minPrice, decimal? maxPrice);
    Product? GetProductById(int productId);
    Product AddProduct(string name, string category, string description, decimal price, int stockQuantity);
    void UpdateProduct(int productId, string name, string category, string description, decimal price);
    bool DeleteProduct(int productId);
    void RestockProduct(int productId, int quantityToAdd);
    void AddReview(int productId, Customer customer, int rating, string comment);
    IEnumerable<Product> GetLowStockProducts(int threshold);
    IEnumerable<Product> GetPurchasedProducts(Customer customer);
}
