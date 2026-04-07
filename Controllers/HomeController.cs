using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CuoiKy.Models;
using CuoiKy.Data;
using Microsoft.EntityFrameworkCore;
using CuoiKy.ViewModels;

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

        var vm = new HomeIndexViewModel
        {
            Categories = categories,
            Products = products
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
