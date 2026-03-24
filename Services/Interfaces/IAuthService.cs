using CollabSpace.Models.DTOs.Auth;

namespace CollabSpace.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
    }
}
