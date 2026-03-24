using System.ComponentModel.DataAnnotations;

namespace CuoiKy.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
}

