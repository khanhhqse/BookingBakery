using BookingBakery.Application.DTO;
using BookingBakery.Application.IService;
using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using BookingBakery.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BookingBakery.Application.Service
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _userRepository;
        private readonly IUserProfileRepository _profileRepository;
        private readonly IConfiguration _configuration;

        public AuthService(
            IAuthRepository userRepository,
            IUserProfileRepository profileRepository,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _profileRepository = profileRepository;
            _configuration = configuration;
        }

        public async Task<string> RegisterAsync(RegisterDto dto, int roleId)
        {
            // Kiểm tra email đã tồn tại chưa
            var existingUser = await _userRepository.FindOneAsync(u => u.Email == dto.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Email đã được sử dụng.");

            // Kiểm tra username đã tồn tại chưa 
            
            var existingUsername = await _userRepository.FindOneAsync(u => u.Username == dto.Username);
            if (existingUsername != null)
                throw new InvalidOperationException("Username đã được sử dụng.");

            // Lấy UserId mới (auto-increment đơn giản)
            var allUsers = await _userRepository.GetAllAsync();
            var newUserId = allUsers.Any() ? allUsers.Max(u => u.UserId) + 1 : 1;

            var user = new User
            {
                UserId = newUserId,
                RoleId = roleId,
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Phone = dto.Phone,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user);

            // ─── Tự tạo profile mặc định, full_name = username ──────
            var allProfiles = await _profileRepository.GetAllAsync();
            var newProfileId = allProfiles.Any() ? allProfiles.Max(p => p.ProfileId) + 1 : 1;

            var profile = new UserProfile
            {
                ProfileId = newProfileId,
                UserId = newUserId,
                FullName = dto.Username,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _profileRepository.CreateAsync(profile);

            return "Đăng ký thành công.";
        }

        public async Task<LoginResultDto?> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.FindOneAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            return GenerateLoginResult(user);
        }

        public async Task<IEnumerable<UserResponseDto>> GetUsersByRoleAsync(int roleId)
        {
            var allUsers = await _userRepository.GetAllAsync();
            return allUsers
                .Where(u => u.RoleId == roleId)
                .Select(MapToUserResponseDto);
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(
    int userId, ChangePasswordDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return (false, "Không tìm thấy tài khoản.");

            // Xác minh mật khẩu cũ
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return (false, "Mật khẩu hiện tại không đúng. Vui lòng kiểm tra lại.");

            // Không cho đặt mật khẩu mới trùng mật khẩu cũ
            if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
                return (false, "Mật khẩu mới không được trùng với mật khẩu hiện tại.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(u => u.UserId == userId, user);

            return (true, "Đổi mật khẩu thành công. Vui lòng đăng nhập lại bằng mật khẩu mới nhé!");
        }

        private static UserResponseDto MapToUserResponseDto(User user)
        {
            return new UserResponseDto
            {
                UserId = user.UserId,
                RoleId = user.RoleId,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        private LoginResultDto GenerateLoginResult(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.RoleId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new LoginResultDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                TokenType = "Bearer",
                ExpiresInMinutes = expiryMinutes,
                Username = user.Username,
                Email = user.Email,
                RoleId = user.RoleId
            };
        }
    }
}