using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingBakery.Domain.Models
{
    public class ProductIngredient
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ObjectId { get; set; }

        [BsonElement("product_id")]
        public int ProductId { get; set; }

        [BsonElement("ingredient_id")]
        public int IngredientId { get; set; }

        [BsonElement("quantity_required")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal QuantityRequired { get; set; }
    }
}
