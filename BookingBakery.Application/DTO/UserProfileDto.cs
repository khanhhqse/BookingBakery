namespace BookingBakery.Application.DTO
{
    public class UpdateUserProfileDto
    {
        public string? FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class UserProfileResponseDto
    {
        public int ProfileId { get; set; }
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}