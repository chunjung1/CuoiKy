using System.ComponentModel.DataAnnotations;

namespace CuoiKy.Models;

public class ProductReview
{
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }

    public Product? Product { get; set; }

    [Required]
    public int UserId { get; set; }

    public User? User { get; set; }

    [Range(1, 5, ErrorMessage = "Vui lòng chọn đánh giá từ 1 đến 5 sao")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nhận xét của bạn")]
    [StringLength(1000, ErrorMessage = "Nhận xét tối đa 1000 ký tự")]
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
