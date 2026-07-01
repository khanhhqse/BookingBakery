using System.ComponentModel.DataAnnotations;

namespace BookingBakery.Application.DTO
{
    public enum PaymentMethodOption
    {
        COD = 1,
        BankTransfer = 2
    }

    public enum OrderStatusOption
    {
        DangLam = 1,
        DangGiao = 2,
        HoanThanh = 3
    }

    // ─────────────────────────────────────────────────────────────
    // REQUEST
    // ─────────────────────────────────────────────────────────────

    public class PlaceOrderRequest
    {
        /// <summary>
        /// Để trống ("" hoặc null) → tự lấy từ UserProfile.Address.
        /// </summary>
        public string? ShippingAddress { get; set; }

        /// <summary>
        /// Để trống ("" hoặc null) → tự lấy từ User.Phone.
        /// </summary>
        public string? Phone { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string? Note { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        public PaymentMethodOption PaymentMethod { get; set; }
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

    /// <summary>Customer cập nhật SĐT và địa chỉ khi đơn đang ở "Chờ xác nhận".</summary>
    public class UpdateOrderContactRequest
    {
        [StringLength(255, MinimumLength = 10,
            ErrorMessage = "Địa chỉ giao hàng phải từ 10 đến 255 ký tự.")]
        public string? ShippingAddress { get; set; }

        public string? Phone { get; set; }
    }

    /// <summary>Filter tìm kiếm đơn hàng (Admin/Staff).</summary>
    public class GetAllOrdersRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Status { get; set; }
        public bool? Today { get; set; }
        public DateTime? FromDate { get; set; }
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
        /// <summary>Hiển thị tên thay vì ID để thân thiện hơn.</summary>
        public string ChangedByUserName { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class OrderSummaryResponse
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int TotalItems { get; set; }
        public int TotalQuantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? CancelReason { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public OrderStatusHistoryResponse? LastStatusChange { get; set; }
    }

    public class OrderResponse
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public List<OrderItemResponse> Items { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? CancelReason { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderStatusHistoryResponse> StatusHistory { get; set; } = new();
    }
}