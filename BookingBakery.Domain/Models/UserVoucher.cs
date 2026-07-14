using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingBakery.Domain.Models
{
    /// <summary>Voucher gắn với 1 user cụ thể — dùng cho voucher được gán trước (minigame) và để lưu lịch sử đã/chưa sử dụng.</summary>
    [BsonIgnoreExtraElements]
    public class UserVoucher
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ObjectId { get; set; }

        [BsonElement("voucher_id")]
        public int VoucherId { get; set; }

        [BsonElement("user_id")]
        public int UserId { get; set; }

        [BsonElement("assigned_at")]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("used_at")]
        public DateTime? UsedAt { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "unused"; // unused/used
    }
}