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

        [DynamoDBProperty("index-instructor")]
        public string? IndexInstructor { get; set; }
    }
}
