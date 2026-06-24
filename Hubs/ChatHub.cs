using Microsoft.AspNetCore.SignalR;
using CuoiKy.Data;
using CuoiKy.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace CuoiKy.Hubs;

public class ChatHub : Hub
{
    private readonly ApplicationDbContext _dbContext;

    public ChatHub(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task OnConnectedAsync()
    {
        var user = Context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleStr = user.FindFirst(ClaimTypes.Role)?.Value;

            if (roleStr == "Admin")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }
            else if (!string.IsNullOrEmpty(userIdStr))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Customer_{userIdStr}");
            }
        }
        await base.OnConnectedAsync();
    }

    public async Task SendMessageToAdmin(string content)
    {
        var user = Context.User;
        if (user?.Identity?.IsAuthenticated != true) return;

        var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int senderId)) return;

        var username = user.Identity.Name ?? "Customer";

        var chatMessage = new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = null,
            Content = content,
            Timestamp = DateTime.Now,
            IsRead = false
        };

        _dbContext.ChatMessages.Add(chatMessage);
        await _dbContext.SaveChangesAsync();

        // Gửi cho nhóm Admin
        await Clients.Group("Admins").SendAsync("ReceiveMessage", senderId, username, content, chatMessage.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss"), chatMessage.Id);

        // Gửi lại cho chính Customer (để đồng bộ các tab nếu có)
        await Clients.Group($"Customer_{senderId}").SendAsync("ReceiveMessage", senderId, username, content, chatMessage.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss"), chatMessage.Id);
    }

    public async Task SendMessageToCustomer(int customerId, string content)
    {
        var user = Context.User;
        if (user?.Identity?.IsAuthenticated != true || !user.IsInRole("Admin")) return;

        var adminIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminIdStr) || !int.TryParse(adminIdStr, out int adminId)) return;

        var adminUsername = user.Identity.Name ?? "Admin";

        var chatMessage = new ChatMessage
        {
            SenderId = adminId,
            ReceiverId = customerId,
            Content = content,
            Timestamp = DateTime.Now,
            IsRead = false
        };

        _dbContext.ChatMessages.Add(chatMessage);
        await _dbContext.SaveChangesAsync();

        // Gửi tới Khách hàng
        await Clients.Group($"Customer_{customerId}").SendAsync("ReceiveMessage", adminId, adminUsername, content, chatMessage.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss"), chatMessage.Id);

        // Gửi tới tất cả Admin khác để cập nhật giao diện dashboard
        await Clients.Group("Admins").SendAsync("ReceiveMessageFromAdmin", customerId, adminId, adminUsername, content, chatMessage.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss"), chatMessage.Id);
    }
}
