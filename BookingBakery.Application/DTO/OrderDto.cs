using System.ComponentModel.DataAnnotations;

namespace BookingBakery.Application.DTO
{
    /// <summary>Các trạng thái hợp lệ khi cập nhật đơn hàng (theo chiều thuận BR-L01).</summary>
    public enum OrderStatusOption
    {
        /// <summary>Bắt đầu chế biến — hệ thống tự động trừ stock (BR-L02)</summary>
        DangLam = 1,
        /// <summary>Đang giao hàng cho khách</summary>
        DangGiao = 2,
        /// <summary>Hoàn thành đơn hàng</summary>
        HoanThanh = 3
    }
    // ─────────────────────────────────────────────────────────────
    // REQUEST
    // ─────────────────────────────────────────────────────────────

    public class PlaceOrderRequest
    {
        [Required(ErrorMessage = "Vui lòng cung cấp địa chỉ giao hàng.")]
        [StringLength(255, MinimumLength = 10,
            ErrorMessage = "Địa chỉ giao hàng phải từ 10 đến 255 ký tự.")]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string? Note { get; set; }
    }

    public class CancelOrderRequest
    {
        [Required(ErrorMessage = "Vui lòng cung cấp lý do hủy đơn.")]
        [StringLength(500, MinimumLength = 5,
            ErrorMessage = "Lý do hủy phải từ 5 đến 500 ký tự.")]
        public string CancelReason { get; set; } = string.Empty;
    }

    public class UpdateOrderStatusRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn trạng thái mới.")]
        public OrderStatusOption NewStatus { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string? Note { get; set; }
    }

    /// <summary>Query filter cho Admin/Staff xem danh sách đơn hàng.</summary>
    public class GetAllOrdersRequest
    {
        /// <summary>Trang hiện tại (mặc định 1).</summary>
        public int Page { get; set; } = 1;

        /// <summary>Số đơn mỗi trang (mặc định 20, tối đa 100).</summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Lọc theo trạng thái đơn hàng.
        /// Giá trị hợp lệ: "Chờ xác nhận" | "Đang làm" | "Đang giao" | "Hoàn thành" | "Đã hủy"
        /// Để trống = lấy tất cả trạng thái.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>Lọc đơn hàng trong ngày hôm nay (giờ VN). Ưu tiên hơn FromDate/ToDate.</summary>
        public bool? Today { get; set; }

        /// <summary>Lọc từ ngày (định dạng: yyyy-MM-dd). Ví dụ: 2026-06-01</summary>
        public DateTime? FromDate { get; set; }

        /// <summary>Lọc đến ngày (định dạng: yyyy-MM-dd). Ví dụ: 2026-06-30</summary>
        public DateTime? ToDate { get; set; }
    }

    // ─────────────────────────────────────────────────────────────
    // RESPONSE
    // ─────────────────────────────────────────────────────────────

    public class OrderItemResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class OrderStatusHistoryResponse
    {
        public string Status { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public int ChangedByUserId { get; set; }
        public string? Note { get; set; }
    }

    public class OrderResponse
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? CancelReason { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderStatusHistoryResponse> StatusHistory { get; set; } = new();
    }
}