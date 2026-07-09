using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingBakery.Domain.Models
{
    [BsonIgnoreExtraElements]
    public class Voucher
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ObjectId { get; set; }

        [BsonElement("voucher_id")]
        public int VoucherId { get; set; }

        [BsonElement("code")]
        public string Code { get; set; } = string.Empty;

        [BsonElement("description")]
        public string? Description { get; set; }

        // Hiện tại chỉ hỗ trợ percentage, để field sẵn cho mở rộng sau (fixed_amount...)
        [BsonElement("discount_type")]
        public string DiscountType { get; set; } = "percentage";

        [BsonElement("discount_value")]
        public decimal DiscountValue { get; set; } // % giảm, vd 10 = 10%

        [BsonElement("min_order_value")]
        public decimal MinOrderValue { get; set; }

        [BsonElement("max_discount_amount")]
        public decimal MaxDiscountAmount { get; set; } // 0 = không giới hạn trần giảm giá

        [BsonElement("start_date")]
        public DateOnly StartDate { get; set; }

        [BsonElement("end_date")]
        public DateOnly EndDate { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "active"; // active/inactive

        // true = được cộng dồn thêm vào giá đã sale từ promotion
        // false = bỏ qua promotion, tính % giảm trên giá gốc
        [BsonElement("can_combine_with_promotion")]
        public bool CanCombineWithPromotion { get; set; }

        // "AllProducts" | "SpecificProducts"
        [BsonElement("apply_scope")]
        public string ApplyScope { get; set; } = "AllProducts";

        // true = voucher chỉ dùng được khi user đã được gán trước (vd trúng từ minigame) -> check bảng UserVoucher
        // false = voucher công khai, ai nhập đúng code cũng dùng được (nhưng mỗi user chỉ 1 lần)
        [BsonElement("requires_assignment")]
        public bool RequiresAssignment { get; set; } = false;

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}