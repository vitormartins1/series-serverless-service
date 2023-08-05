using Amazon.DynamoDBv2.DataModel;

namespace Series.Serverless.Service.Entities
{
    [DynamoDBTable("SeriesTable")]
    public class Student
    {
        [DynamoDBHashKey]
        public string PK { get; set; }

        [DynamoDBRangeKey]
        public string SK { get; set; }

        [DynamoDBProperty("username")]
        public string Username { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey("index-instructor")]
        public string InstructorIndex { get; set; }
    }
}