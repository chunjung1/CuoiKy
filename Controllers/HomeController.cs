using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CuoiKy.Models;
using CuoiKy.Data;
using Microsoft.EntityFrameworkCore;
using CuoiKy.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace CuoiKy.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _dbContext;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _dbContext.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();

        var products = await _dbContext.Products
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        // 1. Initialize session if not started to ensure stable Session ID
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("SessionStarted")))
        {
            HttpContext.Session.SetString("SessionStarted", "true");
        }
        string sessionId = HttpContext.Session.Id;

        // 2. Identify current user if authenticated
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

        // 3. Retrieve recent search queries within past 7 days
        var searchCutoff = DateTime.UtcNow.AddDays(-7);
        var recentQueries = await _dbContext.SearchHistories
            .Where(s => (userId.HasValue ? s.UserId == userId.Value : s.SessionId == sessionId) && s.SearchedAt >= searchCutoff)
            .OrderByDescending(s => s.SearchedAt)
            .Select(s => s.QueryText)
            .Distinct()
            .Take(5)
            .ToListAsync();

        var recommendedProducts = new List<Product>();

        if (recentQueries.Any())
        {
            var matchingProducts = new List<Product>();

            // Find products matching the search query or their category/brand name
            foreach (var query in recentQueries)
            {
                var matches = await _dbContext.Products
                    .Include(p => p.Category)
                    .Where(p => EF.Functions.Like(p.Name, $"%{query}%") || 
                                 (p.Category != null && EF.Functions.Like(p.Category.Name, $"%{query}%")) ||
                                 (p.BrandId != null && EF.Functions.Like(p.BrandId, $"%{query}%")))
                    .ToListAsync();
                matchingProducts.AddRange(matches);
            }

            var targetProducts = matchingProducts.DistinctBy(p => p.Id).ToList();

            if (targetProducts.Any())
            {
                var categoryIds = targetProducts.Select(p => p.CategoryId).Distinct().ToList();
                var brands = targetProducts.Where(p => !string.IsNullOrEmpty(p.BrandId)).Select(p => p.BrandId).Distinct().ToList();
                var avgPrice = targetProducts.Average(p => p.Price);

                // Query recommendations: same categories or brands, prioritizing discount pricing and upsells (price >= average price)
                recommendedProducts = await _dbContext.Products
                    .Include(p => p.Category)
                    .Where(p => categoryIds.Contains(p.CategoryId) || (p.BrandId != null && brands.Contains(p.BrandId)))
                    .OrderByDescending(p => p.DiscountPrice.HasValue) // Highlight promotions
                    .ThenByDescending(p => p.Price >= avgPrice)       // Promote slightly higher value products for upsell
                    .ThenByDescending(p => p.Id)                      // Keep recommendations fresh
                    .Take(6)
                    .ToListAsync();
            }
        }

        // Fallback: If no queries or matches, fetch discounted or newest items
        if (!recommendedProducts.Any())
        {
            recommendedProducts = await _dbContext.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.DiscountPrice.HasValue)
                .ThenByDescending(p => p.Id)
                .Take(6)
                .ToListAsync();
        }

        var vm = new HomeIndexViewModel
        {
            Categories = categories,
            Products = products,
            RecommendedProducts = recommendedProducts
        };

        return View(vm);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
