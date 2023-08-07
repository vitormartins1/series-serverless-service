using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeriesAPI.Entities
{
    [DynamoDBTable("series")]
    public class Student
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
