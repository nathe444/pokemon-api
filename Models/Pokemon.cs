using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PokemonApi.Models
{
    public class Pokemon
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? MongoId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Type { get; set; } 
        public string Ability { get; set; } = string.Empty;
        public int Level { get; set; }
    }
}
