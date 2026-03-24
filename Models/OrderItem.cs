using System.ComponentModel.DataAnnotations;

namespace CuoiKy.Models;

public class OrderItem
{
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    public decimal UnitPrice { get; set; }

    public decimal TotalPrice => UnitPrice * Quantity;

    // Navigation properties (optional but recommended for EF Core)
    public Order? Order { get; set; }
    public Product? Product { get; set; }
}
