using BookingBakery.Application.DTO;
using BookingBakery.Application.Exceptions;
using BookingBakery.Application.IService;
using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;

namespace BookingBakery.Application.Service
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _profileRepository;
        private readonly IAuthRepository _authRepository;

        private const int MinimumAgeYears = 13;
        private const int MaximumAgeYears = 120;

        public UserProfileService(
            IUserProfileRepository profileRepository,
            IAuthRepository authRepository)
        {
            _profileRepository = profileRepository;
            _authRepository = authRepository;
        }

        public async Task<UserProfileResponseDto?> GetByUserIdAsync(int userId)
        {
            var profile = await _profileRepository.FindOneAsync(p => p.UserId == userId);
            return profile == null ? null : MapToDto(profile);
        }

        public async Task<UserProfileResponseDto> UpsertMyProfileAsync(int userId, UpdateUserProfileDto dto)
        {
            if (dto.Birthday.HasValue)
                ValidateBirthday(dto.Birthday.Value);

            var profile = await _profileRepository.FindOneAsync(p => p.UserId == userId);

            if (profile == null)
            {
                var allProfiles = await _profileRepository.GetAllAsync();
                var newProfileId = allProfiles.Any() ? allProfiles.Max(p => p.ProfileId) + 1 : 1;

                profile = new UserProfile
                {
                    ProfileId = newProfileId,
                    UserId = userId,
                    FullName = dto.FullName?.Trim(),
                    Birthday = dto.Birthday,
                    Gender = dto.Gender,
                    Address = dto.Address?.Trim(),
                    AvatarUrl = dto.AvatarUrl?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _profileRepository.CreateAsync(profile);
            }
            else
            {
                profile.FullName = CoalesceIfProvided(dto.FullName, profile.FullName);
                profile.Birthday = dto.Birthday ?? profile.Birthday;
                profile.Gender = dto.Gender ?? profile.Gender;
                profile.Address = CoalesceIfProvided(dto.Address, profile.Address);
                profile.AvatarUrl = CoalesceIfProvided(dto.AvatarUrl, profile.AvatarUrl);
                profile.UpdatedAt = DateTime.UtcNow;

                await _profileRepository.UpdateAsync(p => p.UserId == userId, profile);
            }

            return MapToDto(profile);
        }

        public async Task<IEnumerable<UserProfileResponseDto>> GetAllAsync()
        {
            var profiles = await _profileRepository.GetAllAsync();
            return profiles.Select(MapToDto);
        }

        public async Task<UserProfileResponseDto?> GetByProfileIdAsync(int profileId)
        {
            var profile = await _profileRepository.GetByIdAsync(profileId);
            return profile == null ? null : MapToDto(profile);
        }

        public async Task<IEnumerable<UserProfileResponseDto>> GetByUserIdsAsync(IEnumerable<int> userIds)
        {
            var idSet = userIds.ToHashSet();
            var allProfiles = await _profileRepository.GetAllAsync();
            return allProfiles.Where(p => idSet.Contains(p.UserId)).Select(MapToDto);
        }

        // ──────────────────────────────────────────────────────────────
        // SEARCH CUSTOMERS
        // ──────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message, List<CustomerSearchResponse>? Results)>
            SearchCustomersAsync(CustomerSearchRequest request)
        {
            // Bước 1: Lấy tất cả User có role Customer (roleId = 3)
            var allCustomers = (await _authRepository.FindManyAsync(u => u.RoleId == 3)).ToList();

            if (!allCustomers.Any())
                return (true, "Không tìm thấy khách hàng nào.", new List<CustomerSearchResponse>());

            // Bước 2: Lọc theo email nếu có (tìm gần đúng, không phân biệt hoa thường)
            if (!string.IsNullOrWhiteSpace(request.Email))
                allCustomers = allCustomers
                    .Where(u => u.Email.Contains(request.Email, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!allCustomers.Any())
                return (true, "Không tìm thấy khách hàng nào khớp với email đã nhập.",
                    new List<CustomerSearchResponse>());

            // Bước 3: Lấy profile của các customer còn lại
            var customerUserIds = allCustomers.Select(u => u.UserId).ToHashSet();
            var profiles = (await _profileRepository.FindManyAsync(
                p => customerUserIds.Contains(p.UserId))).ToList();

            // Bước 4: Join User + UserProfile
            var joined = allCustomers.Select(user =>
            {
                var profile = profiles.FirstOrDefault(p => p.UserId == user.UserId);
                return new CustomerSearchResponse
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Phone = user.Phone,
                    FullName = profile?.FullName,
                    Gender = profile?.Gender,
                    Address = profile?.Address,
                    Birthday = profile?.Birthday,
                    AvatarUrl = profile?.AvatarUrl
                };
            }).ToList();

            // Bước 5: Áp dụng filter trên profile

            if (!string.IsNullOrWhiteSpace(request.Name))
                joined = joined
                    .Where(c => c.FullName != null &&
                                c.FullName.Contains(request.Name, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!string.IsNullOrWhiteSpace(request.Gender))
                joined = joined
                    .Where(c => c.Gender != null &&
                                c.Gender.Equals(request.Gender, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!string.IsNullOrWhiteSpace(request.Address))
                joined = joined
                    .Where(c => c.Address != null &&
                                c.Address.Contains(request.Address, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (request.BirthMonth.HasValue)
                joined = joined
                    .Where(c => c.Birthday.HasValue &&
                                c.Birthday.Value.AddHours(7).Month == request.BirthMonth.Value)
                    .ToList();

            if (request.BirthYear.HasValue)
                joined = joined
                    .Where(c => c.Birthday.HasValue &&
                                c.Birthday.Value.AddHours(7).Year == request.BirthYear.Value)
                    .ToList();

            if (!joined.Any())
                return (true, "Không tìm thấy khách hàng nào khớp với điều kiện tìm kiếm.",
                    new List<CustomerSearchResponse>());

            return (true, $"Tìm thấy {joined.Count} khách hàng.", joined);
        }

        // ──────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ──────────────────────────────────────────────────────────────

        private static string? CoalesceIfProvided(string? newValue, string? oldValue)
            => string.IsNullOrWhiteSpace(newValue) ? oldValue : newValue.Trim();

        private static void ValidateBirthday(DateTime birthday)
        {
            var today = DateTime.UtcNow.Date;
            var birthDate = birthday.Date;

            if (birthDate > today)
                throw new ValidationException(nameof(UpdateUserProfileDto.Birthday),
                    "Ngày sinh không thể ở tương lai.");

            if (birthDate > today.AddYears(-MinimumAgeYears))
                throw new ValidationException(nameof(UpdateUserProfileDto.Birthday),
                    $"Người dùng phải ít nhất {MinimumAgeYears} tuổi.");

            if (birthDate < today.AddYears(-MaximumAgeYears))
                throw new ValidationException(nameof(UpdateUserProfileDto.Birthday),
                    "Ngày sinh không hợp lệ.");
        }

        private static UserProfileResponseDto MapToDto(UserProfile profile) => new()
        {
            ProfileId = profile.ProfileId,
            UserId = profile.UserId,
            FullName = profile.FullName,
            Birthday = profile.Birthday,
            Gender = profile.Gender,
            Address = profile.Address,
            AvatarUrl = profile.AvatarUrl,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }
}