using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingBakery.Domain.Models
{
    public class CartItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ObjectId { get; set; }

        // Composite key: cặp (CartId, ProductId) phải DUY NHẤT,
        // enforce bằng unique compound index ở CartItemRepository.

        [BsonElement("cart_id")]
        public int CartId { get; set; }

        [BsonElement("product_id")]
        public int ProductId { get; set; }

        [BsonElement("quantity")]
        public int Quantity { get; set; }
    }
}