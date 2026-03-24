using System.ComponentModel.DataAnnotations;

namespace CuoiKy.Models;

public enum UserRole
{
    Customer,
    Admin
}

public class User
{
    public int Id { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }

    [Required]
    public UserRole Role { get; set; } = UserRole.Customer;
}
