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

        // "percentage" = giảm theo %, "fixed_amount" = giảm số tiền cố định (vd 500000 = giảm 500k)
        [BsonElement("discount_type")]
        public string DiscountType { get; set; } = "percentage";

        [BsonElement("discount_value")]
        public decimal DiscountValue { get; set; } // percentage: % giảm (vd 10 = 10%) | fixed_amount: số tiền giảm (vd 500000)

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

        [BsonElement("can_combine_with_promotion")]
        public bool CanCombineWithPromotion { get; set; }

        [BsonElement("apply_scope")]
        public string ApplyScope { get; set; } = "AllProducts";

        [BsonElement("requires_assignment")]
        public bool RequiresAssignment { get; set; } = false;

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}