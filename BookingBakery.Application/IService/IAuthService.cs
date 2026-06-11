using BookingBakery.Application.DTO;

namespace BookingBakery.Application.IService
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto dto);
        Task<LoginResultDto?> LoginAsync(LoginDto dto);
    }
}
