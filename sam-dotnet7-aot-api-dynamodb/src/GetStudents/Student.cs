using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetStudents
{
    //[DynamoDBTable("students")]
    [DynamoDBTable("series")]
    public class Student
    {
        [DynamoDBHashKey("PK")]
        public int? PK { get; set; }
        [DynamoDBRangeKey("SK")]
        public string? SK { get; set; }
        [DynamoDBProperty("username")]
        public string? Username { get; set; }
    }
}
