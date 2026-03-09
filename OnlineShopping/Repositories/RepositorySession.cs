using OnlineShopping.Interfaces;
using OnlineShopping.Utilities;

namespace OnlineShopping.Repositories;

public sealed class RepositorySession : IRepositorySession
{
    private readonly AppDataContext _context;

    public RepositorySession(AppDataContext context)
    {
        _context = context;
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }
}
