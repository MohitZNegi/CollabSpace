using CollabSpace.Models.DTOs.WorkSpace;
using CollabSpace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollabSpace.Controllers
{
    [ApiController]
    [Route("api/v1/workspaces")]
    [Authorize]
    public class WorkspaceController : ControllerBase
    {
        private readonly IWorkspaceService _workspaceService;
        private readonly ICurrentUserService _currentUser;

        public WorkspaceController(
            IWorkspaceService workspaceService,
            ICurrentUserService currentUser)
        {
            _workspaceService = workspaceService;
            _currentUser = currentUser;
        }

        // GET /api/v1/workspaces
        [HttpGet]
        public async Task<IActionResult> GetMyWorkspaces()
        {
            var result = await _workspaceService
                .GetMyWorkspacesAsync(_currentUser.GetUserId());
            return Ok(result);
        }

        // GET /api/v1/workspaces/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetWorkspace(Guid id)
        {
            var result = await _workspaceService
                .GetWorkspaceAsync(id, _currentUser.GetUserId());
            return Ok(result);
        }

        // POST /api/v1/workspaces
        [HttpPost]
        public async Task<IActionResult> CreateWorkspace(
            [FromBody] CreateWorkspaceDto request)
        {
            var result = await _workspaceService
                .CreateWorkspaceAsync(request, _currentUser.GetUserId());
            return StatusCode(201, result);
        }

        // POST /api/v1/workspaces/join
        [HttpPost("join")]
        public async Task<IActionResult> JoinWorkspace(
            [FromBody] JoinWorkspaceDto request)
        {
            var result = await _workspaceService
                .JoinByCodeAsync(request.InviteCode, _currentUser.GetUserId());
            return Ok(result);
        }

        // GET /api/v1/workspaces/{id}/members
        [HttpGet("{id:guid}/members")]
        public async Task<IActionResult> GetMembers(Guid id)
        {
            var result = await _workspaceService
                .GetMembersAsync(id, _currentUser.GetUserId());
            return Ok(result);
        }

        // PATCH /api/v1/workspaces/{id}/members/{userId}/role
        [HttpPatch("{id:guid}/members/{userId:guid}/role")]
        public async Task<IActionResult> AssignRole(
            Guid id, Guid userId, [FromBody] AssignRoleDto request)
        {
            await _workspaceService.AssignRoleAsync(
                id, userId, request.Role, _currentUser.GetUserId());
            return NoContent();
        }

        // DELETE /api/v1/workspaces/{id}/members/{userId}
        [HttpDelete("{id:guid}/members/{userId:guid}")]
        public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
        {
            await _workspaceService
                .RemoveMemberAsync(id, userId, _currentUser.GetUserId());
            return NoContent();
        }
    }
}