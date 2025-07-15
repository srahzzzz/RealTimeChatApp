using RealTimeChatApp.Application.DTOs;

namespace RealTimeChatApp.Application.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto registerDto);
        Task<string> LoginAsync(LoginDto loginDto);
    }
}
