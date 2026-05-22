using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Entities;

public class Measurement
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("roomId")]
    public string RoomId { get; set; } = "";

    [BsonElement("timestamp")]
    public DateTime TimestampUtc { get; set; }

    [BsonElement("temperature")]
    public decimal Temperature { get; set; }
    
    [BsonElement("humidity")]
    public decimal Humidity { get; set; }
    
    [BsonElement("lightLevel")]
    public double Light { get; set; }
}