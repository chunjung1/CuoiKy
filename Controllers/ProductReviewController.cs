using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CuoiKy.Data;
using CuoiKy.Models;
using System.Security.Claims;

namespace CuoiKy.Controllers;

[Authorize]
public class ProductReviewController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public ProductReviewController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int rating, string comment)
    {
        var username = User.Identity?.Name;
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username || u.Email == email);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // 1. Verify if user has purchased the product
        var hasPurchased = await _dbContext.Orders
            .AnyAsync(o => o.UserId == user.Id && 
                           (o.Status == OrderStatus.Completed || o.Status == OrderStatus.Paid || o.Status == OrderStatus.Shipping) && 
                           o.Items.Any(i => i.ProductId == productId));

        if (!hasPurchased)
        {
            TempData["ErrorMessage"] = "Bạn chỉ có thể đánh giá các sản phẩm bạn đã mua.";
            return RedirectToAction("Details", "Product", new { id = productId });
        }

        // 2. Verify if user has already reviewed the product
        var alreadyReviewed = await _dbContext.ProductReviews
            .AnyAsync(r => r.ProductId == productId && r.UserId == user.Id);

        if (alreadyReviewed)
        {
            TempData["ErrorMessage"] = "Bạn đã gửi đánh giá cho sản phẩm này rồi.";
            return RedirectToAction("Details", "Product", new { id = productId });
        }

        // 3. Validate rating and comment
        if (rating < 1 || rating > 5)
        {
            TempData["ErrorMessage"] = "Đánh giá từ 1 đến 5 sao.";
            return RedirectToAction("Details", "Product", new { id = productId });
        }

        if (string.IsNullOrWhiteSpace(comment))
        {
            TempData["ErrorMessage"] = "Nội dung nhận xét không được để trống.";
            return RedirectToAction("Details", "Product", new { id = productId });
        }

        var review = new ProductReview
        {
            ProductId = productId,
            UserId = user.Id,
            Rating = rating,
            Comment = comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ProductReviews.Add(review);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Cảm ơn bạn đã gửi đánh giá sản phẩm!";
        return RedirectToAction("Details", "Product", new { id = productId });
    }
}
