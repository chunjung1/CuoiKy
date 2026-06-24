using System.ComponentModel.DataAnnotations;

namespace CuoiKy.Models;

public class ChatMessage
{
    public int Id { get; set; }

    [Required]
    public int SenderId { get; set; }
    public User? Sender { get; set; }

    public int? ReceiverId { get; set; }
    public User? Receiver { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.Now;

    public bool IsRead { get; set; } = false;
}
