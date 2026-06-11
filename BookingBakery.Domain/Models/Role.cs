using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingBakery.Domain.Models
{
    public class Role
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ObjectId { get; set; }

        [BsonElement("role_id")]
        public int RoleId { get; set; }

        [BsonElement("role_name")]
        public string RoleName { get; set; } = string.Empty;
    }
}
