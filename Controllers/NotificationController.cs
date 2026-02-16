using backend_trial.Data;
using backend_trial.Models.Domain;
using backend_trial.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend_trial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly IdeaBoardDbContext _dbContext;

        public NotificationController(IdeaBoardDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Get all notifications for current user
        [HttpGet]
        public async Task<ActionResult> GetMyNotifications()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                {
                    return Unauthorized(new { Message = "User ID not found in token" });
                }

                var notifications = await _dbContext.Notifications
                    .Where(n => n.UserId == userGuid)
                    .Include(n => n.Idea)
                    .Include(n => n.Reviewer)
                    .OrderByDescending(n => n.CreatedDate)
                    .Select(n => new NotificationResponseDto
                    {
                        NotificationId = n.NotificationId,
                        UserId = n.UserId,
                        Type = n.Type.ToString(),
                        Message = n.Message,
                        Status = n.Status.ToString(),
                        CreatedDate = n.CreatedDate,
                        IdeaId = n.IdeaId,
                        IdeaTitle = n.Idea != null ? n.Idea.Title : null,
                        ReviewerId = n.ReviewerId,
                        ReviewerName = n.Reviewer != null ? n.Reviewer.Name : null
                    })
                    .ToListAsync();

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error retrieving notifications", Error = ex.Message });
            }
        }

        // Get unread notifications count
        [HttpGet("unread-count")]
        public async Task<ActionResult> GetUnreadCount()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                {
                    return Unauthorized(new { Message = "User ID not found in token" });
                }

                var count = await _dbContext.Notifications
                    .Where(n => n.UserId == userGuid && n.Status == NotificationStatus.Unread)
                    .CountAsync();

                return Ok(new { UnreadCount = count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error retrieving unread count", Error = ex.Message });
            }
        }

        // Mark notification as read
        [HttpPut("{notificationId}/read")]
        public async Task<ActionResult> MarkAsRead(Guid notificationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                {
                    return Unauthorized(new { Message = "User ID not found in token" });
                }

                var notification = await _dbContext.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userGuid);

                if (notification == null)
                {
                    return NotFound(new { Message = "Notification not found" });
                }

                notification.Status = NotificationStatus.Read;
                _dbContext.Notifications.Update(notification);
                await _dbContext.SaveChangesAsync();

                return Ok(new { Message = "Notification marked as read" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error updating notification", Error = ex.Message });
            }
        }

        // Mark all notifications as read
        [HttpPut("read-all")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                {
                    return Unauthorized(new { Message = "User ID not found in token" });
                }

                var notifications = await _dbContext.Notifications
                    .Where(n => n.UserId == userGuid && n.Status == NotificationStatus.Unread)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.Status = NotificationStatus.Read;
                }

                _dbContext.Notifications.UpdateRange(notifications);
                await _dbContext.SaveChangesAsync();

                return Ok(new { Message = "All notifications marked as read" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error updating notifications", Error = ex.Message });
            }
        }

        // Delete notification
        [HttpDelete("{notificationId}")]
        public async Task<ActionResult> DeleteNotification(Guid notificationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                {
                    return Unauthorized(new { Message = "User ID not found in token" });
                }

                var notification = await _dbContext.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userGuid);

                if (notification == null)
                {
                    return NotFound(new { Message = "Notification not found" });
                }

                _dbContext.Notifications.Remove(notification);
                await _dbContext.SaveChangesAsync();

                return Ok(new { Message = "Notification deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error deleting notification", Error = ex.Message });
            }
        }
    }
}
