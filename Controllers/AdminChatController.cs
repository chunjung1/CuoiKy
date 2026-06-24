using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CuoiKy.Data;
using CuoiKy.Models;
using System.Security.Claims;

namespace CuoiKy.Controllers;

[Authorize(Roles = "Admin")]
public class AdminChatController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public AdminChatController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetConversations()
    {
        // Lấy danh sách khách hàng đã từng nhắn tin, kèm tin nhắn cuối và số lượng chưa đọc
        var conversations = await _dbContext.Users
            .Where(u => u.Role == UserRole.Customer &&
                        _dbContext.ChatMessages.Any(m => m.SenderId == u.Id || m.ReceiverId == u.Id))
            .Select(u => new
            {
                CustomerId = u.Id,
                CustomerName = u.Username,
                CustomerEmail = u.Email,
                LastMessage = _dbContext.ChatMessages
                    .Where(m => m.SenderId == u.Id || m.ReceiverId == u.Id)
                    .OrderByDescending(m => m.Timestamp)
                    .Select(m => m.Content)
                    .FirstOrDefault() ?? "",
                LastMessageTime = _dbContext.ChatMessages
                    .Where(m => m.SenderId == u.Id || m.ReceiverId == u.Id)
                    .OrderByDescending(m => m.Timestamp)
                    .Select(m => m.Timestamp)
                    .FirstOrDefault(),
                UnreadCount = _dbContext.ChatMessages
                    .Count(m => m.SenderId == u.Id && m.ReceiverId == null && !m.IsRead)
            })
            .OrderByDescending(c => c.LastMessageTime)
            .ToListAsync();

        var formattedConversations = conversations.Select(c => new
        {
            c.CustomerId,
            c.CustomerName,
            c.CustomerEmail,
            c.LastMessage,
            LastMessageTime = c.LastMessageTime.ToString("yyyy-MM-ddTHH:mm:ss"),
            c.UnreadCount
        });

        return Json(formattedConversations);
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory(int customerId)
    {
        var customer = await _dbContext.Users.FindAsync(customerId);
        if (customer == null)
        {
            return NotFound();
        }

        // Lấy lịch sử nhắn tin giữa khách hàng này và admin
        var messages = await _dbContext.ChatMessages
            .Include(m => m.Sender)
            .Where(m => m.SenderId == customerId || m.ReceiverId == customerId)
            .OrderBy(m => m.Timestamp)
            .Select(m => new
            {
                m.Id,
                m.SenderId,
                IsOwnMessage = m.Sender != null && m.Sender.Role == UserRole.Admin,
                SenderName = m.Sender != null ? m.Sender.Username : "Hệ thống",
                m.Content,
                Timestamp = m.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss"),
                m.IsRead
            })
            .ToListAsync();

        // Đánh dấu đã đọc các tin nhắn gửi từ khách hàng này tới admin
        var unreadMessages = await _dbContext.ChatMessages
            .Where(m => m.SenderId == customerId && m.ReceiverId == null && !m.IsRead)
            .ToListAsync();

        if (unreadMessages.Any())
        {
            foreach (var msg in unreadMessages)
            {
                msg.IsRead = true;
            }
            await _dbContext.SaveChangesAsync();
        }

        return Json(messages);
    }
}
