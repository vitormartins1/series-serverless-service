using Amazon.DynamoDBv2.DataModel;

namespace SeriesAPI.Entities
{
    [DynamoDBTable("series")]
    public class Instructor
    {
        [DynamoDBHashKey("PK")]
        public string? PK { get; set; }

        [DynamoDBRangeKey("SK")]
        public string? SK { get; set; }

        [DynamoDBProperty("username")]
        public string? Username { get; set; }

        [DynamoDBProperty("GSI1PK")]
        public string? GSI1PK { get; set; }
        
        [DynamoDBProperty("GSI1SK")]
        public string? GSI1SK { get; set; }
    }
}
