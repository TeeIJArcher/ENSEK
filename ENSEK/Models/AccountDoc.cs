using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Models;

[BsonIgnoreExtraElements]
public class AccountDoc {
    [BsonId]
    public long AccountId { get; set; }
    public string? FirstName { get; set; }   
    public string? LastName { get; set; }

}
