using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingBakery.Domain.Models
{
    [BsonIgnoreExtraElements]
    public class Promotion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ObjectId { get; set; }

        [BsonElement("promotion_id")]
        public int PromotionId { get; set; }

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("content")]
        public string? Content { get; set; }

        [BsonElement("banner_url")]
        public string? BannerUrl { get; set; }

        /// <summary>"percent" | "fixed".</summary>
        [BsonElement("discount_type")]
        public string DiscountType { get; set; } = PromotionDiscountType.Percent;

        /// <summary>Giá trị giảm — % (0-100) nếu percent, hoặc số tiền cố định nếu fixed.</summary>
        [BsonElement("discount_value")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal DiscountValue { get; set; }

        [BsonElement("start_date")]
        public DateTime StartDate { get; set; }

        [BsonElement("end_date")]
        public DateTime EndDate { get; set; }

        /// <summary>"active" | "inactive".</summary>
        [BsonElement("status")]
        public string Status { get; set; } = PromotionStatus.Active;

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public static class PromotionStatus
    {
        public const string Active = "active";
        public const string Inactive = "inactive";
    }

    public static class PromotionDiscountType
    {
        public const string Percent = "percent";
        public const string Fixed = "fixed";
    }
}