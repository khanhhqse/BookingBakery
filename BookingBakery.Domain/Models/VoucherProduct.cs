using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingBakery.Domain.Models
{
    /// <summary>Bảng trung gian N-N: sản phẩm nào nằm trong phạm vi áp dụng của voucher (khi ApplyScope = SpecificProducts).</summary>
    [BsonIgnoreExtraElements]
    public class VoucherProduct
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ObjectId { get; set; }

        [BsonElement("voucher_id")]
        public int VoucherId { get; set; }

        [BsonElement("product_id")]
        public int ProductId { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}