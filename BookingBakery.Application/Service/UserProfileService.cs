using BookingBakery.Application.DTO;
using BookingBakery.Application.Exceptions; // ValidationException - xem class bên dưới nếu project chưa có
using BookingBakery.Application.IService;
using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;

namespace BookingBakery.Application.Service
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _profileRepository;

        // Ngưỡng tuổi hợp lý - chỉnh lại theo nghiệp vụ thực tế của bakery, đây chỉ là giá trị mặc định
        private const int MinimumAgeYears = 13;
        private const int MaximumAgeYears = 120;

        public UserProfileService(IUserProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public async Task<UserProfileResponseDto?> GetByUserIdAsync(int userId)
        {
            var profile = await _profileRepository.FindOneAsync(p => p.UserId == userId);
            return profile == null ? null : MapToDto(profile);
        }

        public async Task<UserProfileResponseDto> UpsertMyProfileAsync(int userId, UpdateUserProfileDto dto)
        {
            // Chỉ validate khi client thực sự gửi lên giá trị mới
            if (dto.Birthday.HasValue)
            {
                ValidateBirthday(dto.Birthday.Value);
            }

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

        /// <summary>
        /// Giữ giá trị cũ nếu giá trị mới null hoặc rỗng/toàn khoảng trắng (coi như "không nhập gì").
        /// </summary>
        private static string? CoalesceIfProvided(string? newValue, string? oldValue)
        {
            return string.IsNullOrWhiteSpace(newValue) ? oldValue : newValue.Trim();
        }

        private static void ValidateBirthday(DateTime birthday)
        {
            var today = DateTime.UtcNow.Date;
            var birthDate = birthday.Date;

            if (birthDate > today)
                throw new ValidationException(nameof(UpdateUserProfileDto.Birthday), "Ngày sinh không thể ở tương lai.");

            var earliestAllowed = today.AddYears(-MinimumAgeYears);
            if (birthDate > earliestAllowed)
                throw new ValidationException(nameof(UpdateUserProfileDto.Birthday),
                    $"Ngày sinh không hợp lệ. Người dùng phải ít nhất {MinimumAgeYears} tuổi.");

            var oldestAllowed = today.AddYears(-MaximumAgeYears);
            if (birthDate < oldestAllowed)
                throw new ValidationException(nameof(UpdateUserProfileDto.Birthday), "Ngày sinh không hợp lệ.");
        }

        private static UserProfileResponseDto MapToDto(UserProfile profile)
        {
            return new UserProfileResponseDto
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
}