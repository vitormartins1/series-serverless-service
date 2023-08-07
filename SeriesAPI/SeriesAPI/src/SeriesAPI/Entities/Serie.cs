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

        [DynamoDBProperty("exercices")]
        public string? Exercices { get; set; }

        [DynamoDBProperty("version")]
        public int? Version { get; set; }

        [DynamoDBProperty("GSI1PK")]
        public string? GSI1PK { get; set; }

        [DynamoDBProperty("GSI1SK")]
        public string? GSI1SK { get; set; }
    }
}
