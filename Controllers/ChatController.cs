using CollabSpace.Models.DTOs.Chat;
using CollabSpace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollabSpace.Controllers
{
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ICurrentUserService _currentUser;

        public ChatController(
            IChatService chatService,
            ICurrentUserService currentUser)
        {
            _chatService = chatService;
            _currentUser = currentUser;
        }

        // GET /api/v1/workspaces/{id}/messages
        [HttpGet("api/v1/workspaces/{workspaceId:guid}/messages")]
        public async Task<IActionResult> GetWorkspaceMessages(
            Guid workspaceId,
            [FromQuery] DateTime? before,
            [FromQuery] int limit = 50)
        {
            var result = await _chatService.GetWorkspaceMessagesAsync(
                workspaceId, _currentUser.GetUserId(), before, limit);
            return Ok(result);
        }

        // POST /api/v1/workspaces/{id}/messages
        [HttpPost("api/v1/workspaces/{workspaceId:guid}/messages")]
        public async Task<IActionResult> SendWorkspaceMessage(
            Guid workspaceId, [FromBody] SendMessageDto request)
        {
            var result = await _chatService.SendWorkspaceMessageAsync(
                workspaceId, request, _currentUser.GetUserId());
            return StatusCode(201, result);
        }

        // PATCH /api/v1/messages/{id}
        [HttpPatch("api/v1/messages/{messageId:guid}")]
        public async Task<IActionResult> EditMessage(
            Guid messageId, [FromBody] SendMessageDto request)
        {
            var result = await _chatService.EditWorkspaceMessageAsync(
                messageId, request.Content, _currentUser.GetUserId());
            return Ok(result);
        }

        // GET /api/v1/direct-messages/{userId}
        [HttpGet("api/v1/direct-messages/{userId:guid}")]
        public async Task<IActionResult> GetDirectMessages(
            Guid userId,
            [FromQuery] DateTime? before,
            [FromQuery] int limit = 50)
        {
            var result = await _chatService.GetDirectMessagesAsync(
                userId, _currentUser.GetUserId(), before, limit);
            return Ok(result);
        }

        // POST /api/v1/direct-messages/{userId}
        [HttpPost("api/v1/direct-messages/{userId:guid}")]
        public async Task<IActionResult> SendDirectMessage(
            Guid userId, [FromBody] SendMessageDto request)
        {
            var result = await _chatService.SendDirectMessageAsync(
                userId, request, _currentUser.GetUserId());
            return StatusCode(201, result);
        }

        // PATCH /api/v1/direct-messages/{userId}/read
        [HttpPatch("api/v1/direct-messages/{userId:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid userId)
        {
            await _chatService.MarkDirectMessagesAsReadAsync(
                userId, _currentUser.GetUserId());
            return NoContent();
        }
    }
}