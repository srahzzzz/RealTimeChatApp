using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RealTimeChatApp.Application.DTOs;
using RealTimeChatApp.Application.Interfaces;
using RealTimeChatApp.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using RealTimeChatApp.Infrastructure.Settings;

namespace RealTimeChatApp.Infrastructure.Services
{
  public class AuthService : IAuthService
  {
    private readonly JwtSettings _jwtSettings;

    // temp in-memory store
    private readonly List<User> _users = new();

    public AuthService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public Task<string> RegisterAsync(RegisterDto registerDto)
    {
        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)
        };

        _users.Add(user);

        return Task.FromResult(GenerateToken(user));
    }

    public Task<string> LoginAsync(LoginDto loginDto)
    {
        var user = _users.FirstOrDefault(u => u.Email == loginDto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        return Task.FromResult(GenerateToken(user));
    }

    private string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("username", user.Username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }
}
