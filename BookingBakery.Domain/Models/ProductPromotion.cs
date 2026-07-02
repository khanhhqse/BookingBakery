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

        /// <summary>
        /// Danh sách tên size được áp promotion này.
        /// Null hoặc empty = áp dụng cho TẤT CẢ size của sản phẩm.
        /// VD: ["L", "XL"] = chỉ giảm size L và XL.
        /// </summary>
        [BsonElement("applicable_sizes")]
        public List<string> ApplicableSizes { get; set; } = new();

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}