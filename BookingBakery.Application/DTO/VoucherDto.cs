using System.ComponentModel;

namespace BookingBakery.Application.DTO
{
    // ── Output ─────────────────────────────────────────────
    public class VoucherDto
    {
        public int VoucherId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string DiscountType { get; set; } = "percentage";
        public decimal DiscountValue { get; set; }
        public decimal MinOrderValue { get; set; }
        public decimal MaxDiscountAmount { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string Status { get; set; } = "active";
        public bool CanCombineWithPromotion { get; set; }
        public VoucherApplyScope ApplyScope { get; set; } = VoucherApplyScope.AllProducts;
        public List<int> ProductIds { get; set; } = new();
    }

    // ── Admin request ──────────────────────────────────────
    public class CreateVoucherRequest
    {
        /// <summary>Mã voucher, khách nhập mã này để áp dụng. Viết hoa, không dấu, không khoảng trắng.</summary>
        [DefaultValue("BANHMINGON")]
        public string Code { get; set; } = string.Empty;

        /// <summary>Mô tả voucher hiển thị cho khách xem</summary>
        [DefaultValue("Giảm giá bánh mì")]
        public string? Description { get; set; }

        /// <summary>% giảm giá. VD: 10 nghĩa là giảm 10%</summary>
        [DefaultValue(10)]
        public decimal DiscountValue { get; set; }

        /// <summary>Giá trị đơn hàng tối thiểu (trước giảm) để được áp voucher. 0 = không yêu cầu tối thiểu</summary>
        [DefaultValue(10000)]
        public decimal MinOrderValue { get; set; }

        /// <summary>Số tiền giảm tối đa. 0 = không giới hạn trần</summary>
        [DefaultValue(30000)]
        public decimal MaxDiscountAmount { get; set; }

        /// <summary>Ngày bắt đầu hiệu lực</summary>
        public DateOnly StartDate { get; set; }

        /// <summary>Ngày hết hiệu lực</summary>
        public DateOnly EndDate { get; set; }

        /// <summary>true = được cộng dồn thêm vào giá đã giảm từ promotion. false = bỏ qua promotion, tính % trên giá gốc</summary>
        [DefaultValue(true)]
        public bool CanCombineWithPromotion { get; set; }

        /// <summary>
        /// AllProducts (0) = áp dụng toàn bộ sản phẩm trong đơn — bỏ trống ProductIds.
        /// SpecificProducts (1) = chỉ áp dụng sản phẩm cụ thể — bắt buộc điền ProductIds.
        /// </summary>
        public VoucherApplyScope ApplyScope { get; set; } = VoucherApplyScope.AllProducts;

        /// <summary>Danh sách ProductId áp dụng. CHỈ điền khi ApplyScope = SpecificProducts. Nếu ApplyScope = AllProducts thì để mảng rỗng [] hoặc null</summary>
        public List<int>? ProductIds { get; set; }
    }

    public class UpdateVoucherRequest
    {
        public string? Description { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinOrderValue { get; set; }
        public decimal MaxDiscountAmount { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        /// <summary>active = đang hoạt động, inactive = đã vô hiệu hóa</summary>
        [DefaultValue("active")]
        public string Status { get; set; } = "active";

        public bool CanCombineWithPromotion { get; set; }
        public VoucherApplyScope ApplyScope { get; set; } = VoucherApplyScope.AllProducts;

        /// <summary>Chỉ điền khi ApplyScope = SpecificProducts. Để trống [] nếu AllProducts</summary>
        public List<int>? ProductIds { get; set; }
    }

    // ── Customer: áp voucher ───────────────────────────────
    public class ApplyVoucherRequest
    {
        /// <summary>Mã voucher khách nhập ở bước checkout</summary>
        [DefaultValue("BANHMINGON")]
        public string VoucherCode { get; set; } = string.Empty;
    }

    public class VoucherItemBreakdownDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal PromotionPrice { get; set; }
        public bool IsVoucherApplied { get; set; }
        public decimal FinalPrice { get; set; }
    }

    public class ApplyVoucherResultDto
    {
        public string VoucherCode { get; set; } = string.Empty;
        public List<VoucherItemBreakdownDto> Items { get; set; } = new();
        public decimal SubtotalBeforeDiscount { get; set; }
        public decimal SubtotalAfterPromotion { get; set; }
        public decimal VoucherDiscountAmount { get; set; }
        public decimal TotalPayment { get; set; }
    }
}