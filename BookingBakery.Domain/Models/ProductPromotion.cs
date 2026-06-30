using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingBakery.Domain.Models
{
    /// <summary>Bảng trung gian N-N giữa Promotion và Product.</summary>
    [BsonIgnoreExtraElements]
    public class ProductPromotion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ObjectId { get; set; }

        [BsonElement("promotion_id")]
        public int PromotionId { get; set; }

        [BsonElement("product_id")]
        public int ProductId { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}