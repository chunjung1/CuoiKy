using System.ComponentModel.DataAnnotations;

namespace CuoiKy.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên sản phẩm không được rỗng")]
    [RegularExpression(@"^[^!@#$%^&*]*$", ErrorMessage = "Tên sản phẩm không được chứa kí tự đặc biệt (!@#$%^&*)")]
    [Display(Name = "Tên sản phẩm")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn loại sản phẩm")]
    [Display(Name = "Loại sản phẩm")]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    [Required(ErrorMessage = "Giá sản phẩm không được rỗng")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm không được âm")]
    [Display(Name = "Giá sản phẩm")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Số lượng tồn không được rỗng")]
    [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn ít nhất là 0 (không được âm)")]
    [Display(Name = "Số lượng hàng tồn")]
    public int StockQuantity { get; set; }

    [Display(Name = "Mô tả sản phẩm")]
    public string? Description { get; set; }

    [Display(Name = "Hình ảnh sản phẩm")]
    public string? ImageUrl { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá khuyến mãi không được âm")]
    [Display(Name = "Giá khuyến mãi")]
    public decimal? DiscountPrice { get; set; }

    [Display(Name = "Thương hiệu")]
    public string? BrandId { get; set; }
}
