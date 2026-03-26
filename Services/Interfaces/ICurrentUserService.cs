namespace CollabSpace.Services.Interfaces
{
    public interface ICurrentUserService
    {
        // Returns the user's Id from the JWT claim
        Guid GetUserId();

        // Returns the user's global role from the JWT claim
        string GetUserRole();

        // Convenience check for the Admin global role
        bool IsAdmin();
    }
}
