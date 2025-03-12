using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace MicroservicesWithKafka.Models
{
    public class Fund
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int FundId { get; set; }
        public string? FundName { get; set; }
        public string? FundObjective { get; set; }

    }
}
