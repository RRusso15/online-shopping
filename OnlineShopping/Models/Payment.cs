namespace OnlineShopping.Models;

public sealed class Payment
{
    public Payment(int id, int orderId, string customerUsername, decimal amount, PaymentStatus status, string message)
    {
        Id = id;
        OrderId = orderId;
        CustomerUsername = customerUsername;
        Amount = amount;
        Status = status;
        Message = message;
        PaymentDate = DateTime.Now;
    }

    public int Id { get; }
    public int OrderId { get; }
    public string CustomerUsername { get; }
    public decimal Amount { get; }
    public DateTime PaymentDate { get; }
    public PaymentStatus Status { get; }
    public string Message { get; }
}
