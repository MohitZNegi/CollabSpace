using CollabSpace.Models.DTOs.Comment;
using CollabSpace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollabSpace.Controllers
{
    [ApiController]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ICurrentUserService _currentUser;

        public CommentController(
            ICommentService commentService,
            ICurrentUserService currentUser)
        {
            _commentService = commentService;
            _currentUser = currentUser;
        }

        // GET /api/v1/cards/{cardId}/comments
        [HttpGet("api/v1/cards/{cardId:guid}/comments")]
        public async Task<IActionResult> GetComments(Guid cardId)
        {
            var result = await _commentService.GetCommentsAsync(
                cardId, _currentUser.GetUserId());
            return Ok(result);
        }

        // POST /api/v1/cards/{cardId}/comments
        [HttpPost("api/v1/cards/{cardId:guid}/comments")]
        public async Task<IActionResult> CreateComment(
            Guid cardId, [FromBody] CreateCommentDto request)
        {
            var result = await _commentService.CreateCommentAsync(
                cardId, request, _currentUser.GetUserId());
            return StatusCode(201, result);
        }

        // PATCH /api/v1/comments/{commentId}
        [HttpPatch("api/v1/comments/{commentId:guid}")]
        public async Task<IActionResult> EditComment(
            Guid commentId, [FromBody] EditCommentDto request)
        {
            var result = await _commentService.EditCommentAsync(
                commentId, request, _currentUser.GetUserId());
            return Ok(result);
        }

        // DELETE /api/v1/comments/{commentId}
        [HttpDelete("api/v1/comments/{commentId:guid}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            await _commentService.DeleteCommentAsync(
                commentId, _currentUser.GetUserId());
            return NoContent();
        }
    }
}