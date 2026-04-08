using System.ComponentModel.DataAnnotations;

namespace CuoiKy.Models;

public enum OrderStatus
{
    Pending,
    Paid,
    Shipping,
    Completed,
    Canceled
}

public enum PaymentMethod
{
    Cash,
    BankTransfer
}

public class Order
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public User? User { get; set; }

    public bool GiftWrap { get; set; }

    public bool ExpressShipping { get; set; }

    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }

    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
}
