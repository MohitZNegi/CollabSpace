using BCrypt.Net;
using CollabSpace.Data;
using CollabSpace.Models.DTOs.Auth;
using CollabSpace.Models;
using CollabSpace.Models.Settings;
using CollabSpace.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CollabSpace.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtSettings _jwtSettings;

        public AuthService(AppDbContext context, IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            // Check if email already exists
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == request.Email.ToLower());

            if (emailExists)
                throw new InvalidOperationException("Email is already registered.");

            var usernameExists = await _context.Users
                .AnyAsync(u => u.Username == request.Username);

            if (usernameExists)
                throw new InvalidOperationException("Username is already taken.");

            // Hash the password using BCrypt
            // The second argument is the cost factor (12 = our NFR requirement)
            // Higher cost = slower to compute = harder to brute force
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email.ToLower(),
                PasswordHash = passwordHash,
                GlobalRole = "Member",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

            // Always return the same generic error for both wrong email
            // and wrong password. Saying "email not found" tells an
            // attacker which emails are registered in your system.
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is inactive.");

            user.LastSeenAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow || storedToken.IsRevoked)
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            // Rotate the refresh token: revoke the old one, issue a new one
            // This limits the damage if a refresh token is ever stolen
            storedToken.IsRevoked = true;
            await _context.SaveChangesAsync();

            return await GenerateAuthResponseAsync(storedToken.User!);
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken != null)
            {
                storedToken.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }

        // Private helper: builds the full auth response with both tokens
        private async Task<AuthResponseDto> GenerateAuthResponseAsync(User user)
        {
            var accessToken = GenerateAccessToken(user);
            var refreshToken = await GenerateAndStoreRefreshTokenAsync(user.Id);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    GlobalRole = user.GlobalRole,
                    AvatarUrl = user.AvatarUrl
                }
            };
        }

        private string GenerateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            // Claims are the pieces of information embedded in the token.
            // The server reads these on every protected request to know
            // who is making the request without hitting the database.
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.GlobalRole),
                new Claim("username", user.Username),
                // Jti is a unique ID for this specific token.
                // Useful if you ever want to blacklist individual tokens.
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> GenerateAndStoreRefreshTokenAsync(Guid userId)
        {
            // A refresh token is just a cryptographically random string.
            // Unlike the access token, it has no embedded data.
            // Its only job is to be unique and impossible to guess.
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(tokenBytes);

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = token,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return token;
        }
    }
}