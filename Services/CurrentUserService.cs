using CollabSpace.Services.Interfaces;
using System.Security.Claims;

namespace CollabSpace.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // IHttpContextAccessor gives access to the current HTTP request
        // context from inside a service. Without it, services have no
        // way to read HttpContext because they are not controllers.
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid GetUserId()
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (claim == null)
                throw new UnauthorizedAccessException("User is not authenticated.");

            return Guid.Parse(claim);
        }

        public string GetUserRole()
        {
            return _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Role)?.Value ?? "Member";
        }

        public bool IsAdmin() => GetUserRole() == "Admin";
    }
}
