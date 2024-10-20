using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Models;

[BsonIgnoreExtraElements]
public class ReadingDoc {
    [BsonId]
    public string ReadingId { get; } = Guid.NewGuid().ToString();

    public long AccountId { get; set; }
    public string? MeterReadingDateTime { get; set; }   
    public long? MeterReadValue { get; set; }

    public DateTime? ReadingDateTime { get {
        return DateTime.ParseExact(MeterReadingDateTime, "dd/MM/yyyy HH:mm",
                                       System.Globalization.CultureInfo.InvariantCulture);;
    } }  
}
