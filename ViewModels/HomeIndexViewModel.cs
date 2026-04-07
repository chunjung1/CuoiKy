using CuoiKy.Models;

namespace CuoiKy.ViewModels;

public class HomeIndexViewModel
{
    public List<Category> Categories { get; set; } = new();
    public List<Product> Products { get; set; } = new();
}

