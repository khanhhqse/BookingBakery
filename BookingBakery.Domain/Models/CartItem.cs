using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingBakery.Domain.Models
{
    [BsonIgnoreExtraElements]
    public class CartItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ObjectId { get; set; }

        // Composite key: (CartId, ProductId) — unique vì mỗi ProductId đã là 1 size riêng
        [BsonElement("cart_id")]
        public int CartId { get; set; }

        [BsonElement("product_id")]
        public int ProductId { get; set; }

        [BsonElement("quantity")]
        public int Quantity { get; set; }
    }
}