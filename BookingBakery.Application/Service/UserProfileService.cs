using BookingBakery.Application.DTO;
using BookingBakery.Application.IService;
using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;

namespace BookingBakery.Application.Service
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _profileRepository;

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
            var profile = await _profileRepository.FindOneAsync(p => p.UserId == userId);

            if (profile == null)
            {
                var allProfiles = await _profileRepository.GetAllAsync();
                var newProfileId = allProfiles.Any() ? allProfiles.Max(p => p.ProfileId) + 1 : 1;

                profile = new UserProfile
                {
                    ProfileId = newProfileId,
                    UserId = userId,
                    FullName = dto.FullName,
                    Birthday = dto.Birthday,
                    Gender = dto.Gender,
                    Address = dto.Address,
                    AvatarUrl = dto.AvatarUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _profileRepository.CreateAsync(profile);
            }
            else
            {
                profile.FullName = dto.FullName ?? profile.FullName;
                profile.Birthday = dto.Birthday ?? profile.Birthday;
                profile.Gender = dto.Gender ?? profile.Gender;
                profile.Address = dto.Address ?? profile.Address;
                profile.AvatarUrl = dto.AvatarUrl ?? profile.AvatarUrl;
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