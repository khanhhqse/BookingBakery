using BookingBakery.Application.DTO;
using BookingBakery.Application.IService;
using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
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
        private readonly IConfiguration _configuration;

        public AuthService(IAuthRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<string> RegisterAsync(RegisterDto dto)
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
                RoleId = dto.RoleId,
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Phone = dto.Phone,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user);
            return "Đăng ký thành công.";
        }

        public async Task<LoginResultDto?> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.FindOneAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            return GenerateLoginResult(user);
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
