using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CuoiKy.Data;
using CuoiKy.Models;
using System.Security.Claims;

namespace CuoiKy.Controllers;

[Authorize]
public class ChatController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public ChatController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized();
        }

        // Lấy danh sách tin nhắn giữa user này và admin
        var messages = await _dbContext.ChatMessages
            .Include(m => m.Sender)
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .OrderBy(m => m.Timestamp)
            .Select(m => new
            {
                m.Id,
                m.SenderId,
                IsOwnMessage = m.SenderId == userId,
                SenderName = m.Sender != null ? m.Sender.Username : "Người dùng",
                m.Content,
                Timestamp = m.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss"),
                m.IsRead
            })
            .ToListAsync();

        return Json(messages);
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsRead()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized();
        }

        // Đánh dấu đã đọc các tin nhắn gửi tới khách hàng này mà chưa đọc
        var unreadMessages = await _dbContext.ChatMessages
            .Where(m => m.ReceiverId == userId && !m.IsRead)
            .ToListAsync();

        if (unreadMessages.Any())
        {
            foreach (var msg in unreadMessages)
            {
                msg.IsRead = true;
            }
            await _dbContext.SaveChangesAsync();
        }

        return Ok();
    }
}
