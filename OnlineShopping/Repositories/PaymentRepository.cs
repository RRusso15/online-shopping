using OnlineShopping.Interfaces;
using OnlineShopping.Models;
using OnlineShopping.Utilities;

namespace OnlineShopping.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly AppDataContext _context;

    public PaymentRepository(AppDataContext context)
    {
        _context = context;
    }

    public int NextId() => _context.NextPaymentId();

    public void Add(Payment payment)
    {
        _context.Payments.Add(payment);
    }
}
