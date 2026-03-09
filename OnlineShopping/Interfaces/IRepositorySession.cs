namespace OnlineShopping.Interfaces;

/// <summary>
/// Unit-of-work style persistence boundary used by services/repositories.
/// </summary>
public interface IRepositorySession
{
    void SaveChanges();
}
