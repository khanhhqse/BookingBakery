using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingBakery.Domain.Models
{
    public class Ingredient
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ObjectId { get; set; }

        [BsonElement("ingredient_id")]
        public int IngredientId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("unit")]
        public string Unit { get; set; } = string.Empty;

        [BsonElement("current_stock")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal CurrentStock { get; set; }

        [BsonElement("cost_per_unit")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal CostPerUnit { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
