using CollabSpace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollabSpace.Controllers
{
    [ApiController]
    [Route("api/v1/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUser;

        public NotificationController(
            INotificationService notificationService,
            ICurrentUserService currentUser)
        {
            _notificationService = notificationService;
            _currentUser = currentUser;
        }

        // GET /api/v1/notifications
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var result = await _notificationService
                .GetUserNotificationsAsync(_currentUser.GetUserId());
            return Ok(result);
        }

        // GET /api/v1/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _notificationService
                .GetUnreadCountAsync(_currentUser.GetUserId());
            return Ok(new { count });
        }

        // PATCH /api/v1/notifications/read-all
        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _notificationService
                .MarkAllAsReadAsync(_currentUser.GetUserId());
            return NoContent();
        }
    }
}