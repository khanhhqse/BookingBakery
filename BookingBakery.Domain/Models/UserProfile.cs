using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingBakery.Domain.Models
{
    public class UserProfile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ObjectId { get; set; }

        [BsonElement("profile_id")]
        public int ProfileId { get; set; }

        [BsonElement("user_id")]
        public int UserId { get; set; }

        [BsonElement("full_name")]
        public string? FullName { get; set; }

        [BsonElement("birthday")]
        public DateTime? Birthday { get; set; }

        [BsonElement("gender")]
        public string? Gender { get; set; }

        [BsonElement("address")]
        public string? Address { get; set; }

        [BsonElement("avatar_url")]
        public string? AvatarUrl { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}