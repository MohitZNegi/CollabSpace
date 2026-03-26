using CollabSpace.Data;
using CollabSpace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollabSpace.Controllers
{
    [ApiController]
    [Route("api/v1/workspaces")]
    [Authorize] // every endpoint in this controller requires authentication
    public class WorkspaceController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IWorkspaceAuthService _workspaceAuth;

        public WorkspaceController(
            AppDbContext context,
            ICurrentUserService currentUser,
            IWorkspaceAuthService workspaceAuth)
        {
            _context = context;
            _currentUser = currentUser;
            _workspaceAuth = workspaceAuth;
        }

        // GET /api/v1/workspaces
        // Returns all workspaces the current user belongs to
        [HttpGet]
        public async Task<IActionResult> GetMyWorkspaces()
        {
            var userId = _currentUser.GetUserId();

            var workspaces = await _context.WorkspaceMembers
                .Where(wm => wm.UserId == userId)
                .Include(wm => wm.Workspace)
                .Select(wm => new
                {
                    wm.Workspace!.Id,
                    wm.Workspace.Name,
                    wm.Workspace.Description,
                    wm.WorkspaceRole,
                    wm.JoinedAt
                })
                .ToListAsync();

            return Ok(workspaces);
        }

        // GET /api/v1/workspaces/{id}
        // Returns a single workspace — only if the user is a member
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetWorkspace(Guid id)
        {
            var userId = _currentUser.GetUserId();

            // This throws UnauthorizedAccessException if not a member.
            // The global exception handler converts it to a 401 response.
            await _workspaceAuth.RequireMemberAsync(id, userId);

            var workspace = await _context.Workspaces
                .FirstOrDefaultAsync(w => w.Id == id);

            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            return Ok(workspace);
        }
    }
}