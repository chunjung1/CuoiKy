using System.ComponentModel.DataAnnotations;

namespace CuoiKy.Models;

public class Coupon
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Mã giảm giá không được rỗng")]
    [Display(Name = "Mã giảm giá")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mức giảm giá (%) không được rỗng")]
    [Range(1, 100, ErrorMessage = "Mức giảm giá phải từ 1% đến 100%")]
    [Display(Name = "Mức giảm giá (%)")]
    public int DiscountPercent { get; set; }

    [Required(ErrorMessage = "Ngày hết hạn không được rỗng")]
    [Display(Name = "Ngày hết hạn")]
    public DateTime ExpiryDate { get; set; }

    [Required(ErrorMessage = "Giới hạn sử dụng không được rỗng")]
    [Range(1, int.MaxValue, ErrorMessage = "Giới hạn sử dụng phải lớn hơn 0")]
    [Display(Name = "Giới hạn sử dụng")]
    public int UsageLimit { get; set; }

    [Display(Name = "Số lần đã sử dụng")]
    public int UsedCount { get; set; } = 0;
}
