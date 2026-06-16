namespace BookingBakery.Application.DTO
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}