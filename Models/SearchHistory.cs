using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CuoiKy.Models
{
    public class SearchHistory
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        [StringLength(100)]
        public string SessionId { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string QueryText { get; set; } = string.Empty;

        public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
    }
}
