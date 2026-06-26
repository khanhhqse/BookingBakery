namespace BookingBakery.Application.DTO
{
    /// <summary>Filter tìm kiếm khách hàng — tất cả field đều optional, có thể kết hợp.</summary>
    public class CustomerSearchRequest
    {
        /// <summary>Tìm theo tên (không phân biệt hoa thường, tìm gần đúng).</summary>
        public string? Name { get; set; }

        /// <summary>Tìm theo email (không phân biệt hoa thường, tìm gần đúng).</summary>
        public string? Email { get; set; }

        /// <summary>Lọc theo giới tính. Giá trị: "Nam" | "Nữ" | "Khác"</summary>
        public string? Gender { get; set; }

        /// <summary>Lọc theo địa chỉ (tìm gần đúng, VD: "Quận 10" sẽ khớp "123 Tô Hiến Thành, Quận 10").</summary>
        public string? Address { get; set; }

        /// <summary>Lọc khách sinh trong tháng này (1-12). VD: 6 = tháng 6.</summary>
        public int? BirthMonth { get; set; }

        /// <summary>Lọc khách sinh trong năm này. VD: 2000.</summary>
        public int? BirthYear { get; set; }
    }

    /// <summary>Kết quả tìm kiếm — gộp thông tin từ User và UserProfile.</summary>
    public class CustomerSearchResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? FullName { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public DateTime? Birthday { get; set; }
        public string? AvatarUrl { get; set; }
    }
}