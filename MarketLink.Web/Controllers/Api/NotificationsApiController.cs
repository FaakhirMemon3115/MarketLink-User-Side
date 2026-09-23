using Microsoft.AspNetCore.Mvc;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MarketLink.Web.Controllers.Api;

[ApiController]
[Route("api/notifications")]
[Produces("application/json")]
public class NotificationsApiController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationsApiController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserNotifications(string userId)
    {
        var notifs = await _unitOfWork.Repository<Notification>().Query()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new
            {
                n.Id,
                n.Title,
                n.Message,
                n.Link,
                n.IsRead,
                n.Type,
                n.CreatedAt
            })
            .ToListAsync();

        return Ok(new { success = true, unreadCount = notifs.Count(n => !n.IsRead), notifications = notifs });
    }

    [HttpPost("mark-read/{id:int}")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var notif = await _unitOfWork.Repository<Notification>().GetByIdAsync(id);
        if (notif == null) return NotFound(new { success = false, message = "Notification not found." });

        notif.IsRead = true;
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { success = true, message = "Marked as read." });
    }
}
