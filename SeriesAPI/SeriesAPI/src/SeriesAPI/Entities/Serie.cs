using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeriesAPI.Entities
{
    [DynamoDBTable("series")]
    public class Serie
    {
        [DynamoDBHashKey("PK")]
        public string? PK { get; set; }

        [DynamoDBRangeKey("SK")]
        public string? SK { get; set; }

        [DynamoDBProperty("date")]
        public string? Date { get; set; }

        [DynamoDBProperty("index-instructor")]
        public string? IndexInstructor { get; set; }

        [DynamoDBProperty("exercices")]
        public string? Exercices { get; set; }

        [DynamoDBProperty("version")]
        public int? Version { get; set; }
    }
}
