using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CuoiKy.Data;
using CuoiKy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CuoiKy.Controllers;

[Authorize(Roles = "Admin")]
public class ProductController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductController(ApplicationDbContext dbContext, IWebHostEnvironment webHostEnvironment)
    {
        _dbContext = dbContext;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index()
    {
        var products = _dbContext.Products.Include(p => p.Category).ToList();
        return View(products);
    }

    public IActionResult Create()
    {
        ViewBag.CategorySelectList = new SelectList(_dbContext.Categories.OrderBy(c => c.Name).ToList(), "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
    {
        ViewBag.CategorySelectList = new SelectList(_dbContext.Categories.OrderBy(c => c.Name).ToList(), "Id", "Name", product.CategoryId);
        if (ModelState.IsValid)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                product.ImageUrl = await SaveImage(imageFile);
            }

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

    public IActionResult Edit(int id)
    {
        var product = _dbContext.Products.Find(id);
        if (product == null) return NotFound();
        ViewBag.CategorySelectList = new SelectList(_dbContext.Categories.OrderBy(c => c.Name).ToList(), "Id", "Name", product.CategoryId);
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
    {
        if (id != product.Id) return NotFound();
        ViewBag.CategorySelectList = new SelectList(_dbContext.Categories.OrderBy(c => c.Name).ToList(), "Id", "Name", product.CategoryId);

        if (ModelState.IsValid)
        {
            var existingProduct = await _dbContext.Products.FindAsync(id);
            if (existingProduct == null) return NotFound();

            existingProduct.Name = product.Name;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.Price = product.Price;
            existingProduct.DiscountPrice = product.DiscountPrice;
            existingProduct.BrandId = product.BrandId;
            existingProduct.StockQuantity = product.StockQuantity;
            existingProduct.Description = product.Description;

            if (imageFile != null && imageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    DeleteImage(existingProduct.ImageUrl);
                }
                existingProduct.ImageUrl = await SaveImage(imageFile);
            }

            _dbContext.Products.Update(existingProduct);
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

    [AllowAnonymous]
    public async Task<IActionResult> List(string? category, string? q)
    {
        var query = _dbContext.Products.Include(p => p.Category).AsQueryable();
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category != null && p.Category.Name == category);
        }
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{q}%"));

            // Trace/Track search action
            try
            {
                // Ensure session is started to obtain a stable Session ID
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("SessionStarted")))
                {
                    HttpContext.Session.SetString("SessionStarted", "true");
                }
                string sessionId = HttpContext.Session.Id;

                int? userId = null;
                if (User.Identity?.IsAuthenticated == true)
                {
                    var username = User.Identity.Name;
                    var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
                    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username || u.Email == email);
                    if (user != null)
                    {
                        userId = user.Id;
                    }
                }

                // Check if the same query was already logged in the last 30 seconds for this user/session to prevent redundant logs on reload
                var recentCutoff = DateTime.UtcNow.AddSeconds(-30);
                bool alreadyLogged = await _dbContext.SearchHistories.AnyAsync(s =>
                    (userId.HasValue ? s.UserId == userId.Value : s.SessionId == sessionId) &&
                    s.QueryText == q &&
                    s.SearchedAt >= recentCutoff);

                if (!alreadyLogged)
                {
                    var searchHistory = new SearchHistory
                    {
                        UserId = userId,
                        SessionId = sessionId,
                        QueryText = q,
                        SearchedAt = DateTime.UtcNow
                    };
                    _dbContext.SearchHistories.Add(searchHistory);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log exception internally but don't disrupt search results
                Console.WriteLine($"Error tracking search behavior: {ex.Message}");
            }
        }
        var products = await query.ToListAsync();
        ViewBag.CurrentCategory = category;
        ViewBag.Query = q;
        return View(products);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Suggest(string q)
    {
        q = (q ?? string.Empty).Trim();
        if (q.Length < 2)
        {
            return Json(Array.Empty<object>());
        }

        var items = await _dbContext.Products
            .Where(p => EF.Functions.Like(p.Name, $"%{q}%"))
            .OrderByDescending(p => p.Id)
            .Select(p => new
            {
                id = p.Id,
                name = p.Name,
                imageUrl = p.ImageUrl,
                price = p.Price,
                discountPrice = p.DiscountPrice,
                brandId = p.BrandId,
                stockQuantity = p.StockQuantity
            })
            .Take(6)
            .ToListAsync();

        return Json(items);
    }

    [AllowAnonymous] // Allow anyone to view product details
    public async Task<IActionResult> Details(int id)
    {
        var product = await _dbContext.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        // Fetch reviews
        var reviews = await _dbContext.ProductReviews
            .Include(r => r.User)
            .Where(r => r.ProductId == id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        ViewBag.Reviews = reviews;

        // Check if user is eligible to write a review
        bool canReview = false;
        if (User.Identity?.IsAuthenticated == true)
        {
            var username = User.Identity.Name;
            var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username || u.Email == email);
            if (user != null)
            {
                var hasPurchased = await _dbContext.Orders
                    .AnyAsync(o => o.UserId == user.Id && 
                                   (o.Status == OrderStatus.Completed || o.Status == OrderStatus.Paid || o.Status == OrderStatus.Shipping) && 
                                   o.Items.Any(i => i.ProductId == id));

                if (hasPurchased)
                {
                    var alreadyReviewed = await _dbContext.ProductReviews
                        .AnyAsync(r => r.ProductId == id && r.UserId == user.Id);
                    canReview = !alreadyReviewed;
                }
            }
        }
        ViewBag.CanReview = canReview;

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _dbContext.Products.FindAsync(id);
        if (product == null) return Json(new { success = false, message = "Không tìm thấy sản phẩm" });

        if (!string.IsNullOrEmpty(product.ImageUrl))
        {
            DeleteImage(product.ImageUrl);
        }

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync();
        return Json(new { success = true, message = "Xóa sản phẩm thành công!" });
    }

    private async Task<string> SaveImage(IFormFile imageFile)
    {
        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(fileStream);
        }

        return "/images/products/" + uniqueFileName;
    }

    private void DeleteImage(string imageUrl)
    {
        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }
    }
}
