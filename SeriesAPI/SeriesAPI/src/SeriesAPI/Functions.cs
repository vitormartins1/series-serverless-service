using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using SeriesAPI.Entities;
using System.Net;
using Amazon.DynamoDBv2.Model;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace SeriesAPI
{
    public class Functions
    {
        private ICalculatorService _calculatorService;
        private readonly IDynamoDBContext _dynamoDBContext;

        public Functions(
            ICalculatorService calculatorService, 
            IDynamoDBContext dynamoDBContext)
        {
            _calculatorService = calculatorService;
            _dynamoDBContext = dynamoDBContext;
        }

        [LambdaFunction(ResourceName = "CreateStudentFunction")]
        [HttpApi(LambdaHttpMethod.Post, "/student")]
        public async Task<IHttpResult> CreateStudentAsync([FromBody] Student student, ILambdaContext context)
        {
            string key = $"student#{Guid.NewGuid().ToString()}";

            student.PK = key;
            student.SK = key;
            await _dynamoDBContext.SaveAsync(student);

            return HttpResults.Created(null, student);
        }

        [LambdaFunction(ResourceName = "GetAllSeriesFromStudent")]
        [HttpApi(LambdaHttpMethod.Get, "/student/{studentKey}/serie")]
        public async Task<IHttpResult> GetAllSeriesFromStudentAsync(string studentKey, ILambdaContext context)
        {
            List<Serie> series = await _dynamoDBContext
                .QueryAsync<Serie>($"student#{studentKey}", QueryOperator.BeginsWith, new[] { "instructor#" }).GetRemainingAsync();

            return HttpResults.Ok(series);
        }

        [LambdaFunction(ResourceName = "GetStudentByKey")]
        [HttpApi(LambdaHttpMethod.Get, "/student/{key}")]
        public async Task<IHttpResult> GetStudentByKeyAsync(string key, ILambdaContext context)
        {
            string studentKey = $"student#{key}";

            Student student = await _dynamoDBContext
                .LoadAsync<Student>(studentKey, studentKey);

            return HttpResults.Ok(student);
        }

        [LambdaFunction(ResourceName = "GetAllStudentsFromInstructor")]
        [HttpApi(LambdaHttpMethod.Get, "/instructor/{instructorKey}/student")]
        public async Task<IHttpResult> GetStudentsAsync(string instructorKey, ILambdaContext context)
        {
            List<Student> students = await _dynamoDBContext.QueryAsync<Student>($"instructor#{instructorKey}", QueryOperator.BeginsWith, new[] { "student#" }).GetRemainingAsync();

            return HttpResults.Ok(students);
        }

        [LambdaFunction(ResourceName = "DeleteStudentFunction")]
        [HttpApi(LambdaHttpMethod.Delete, "/student/{key}")]
        public async Task<IHttpResult> DeleteStudentAsync(string key, ILambdaContext context)
        {
            string pk = $"student#{key}";
            string sk = $"student#{key}";

            Student student = await _dynamoDBContext.LoadAsync<Student>(pk, sk);

            if (student != null)
            {
                await _dynamoDBContext.DeleteAsync<Student>(pk, sk);

                return HttpResults.NewResult(HttpStatusCode.NoContent);
            }

            return HttpResults.NotFound();
        }

        [LambdaFunction(ResourceName = "CreateInstructorFunction")]
        [HttpApi(LambdaHttpMethod.Post, "/instructor")]
        public async Task<IHttpResult> CreateInstructorAsync([FromBody] Instructor instructor, ILambdaContext context)
        {
            string key = $"instructor#{Guid.NewGuid().ToString()}";

            instructor.PK = key;
            instructor.SK = key;
            instructor.GSI1PK = key;
            instructor.GSI1SK = key;

            await _dynamoDBContext.SaveAsync(instructor);

            return HttpResults.Created(null, instructor);
        }

        [LambdaFunction(ResourceName = "GetInstructorByKey")]
        [HttpApi(LambdaHttpMethod.Get, "/instructor/{key}")]
        public async Task<IHttpResult> GetInstructorByKeyAsync(string key, ILambdaContext context)
        {
            string instructorKey = $"instructor#{key}";

            Instructor instructor = await _dynamoDBContext
                .LoadAsync<Instructor>(instructorKey, instructorKey);

            return HttpResults.Ok(instructor);
        }

        [LambdaFunction(ResourceName = "CreateSerieFunction")]
        [HttpApi(LambdaHttpMethod.Post, "/serie/{studentKey}/{instructorKey}")]
        public async Task<IHttpResult> CreateSerieAsync(
            [FromBody] Serie serie, string studentKey, string instructorKey, ILambdaContext context)
        {
            string pk = $"student#{studentKey}";

            // get count of series from the same student to make a new version of the serie
            //var request = new QueryRequest
            //{
            //    TableName = "series",
            //    KeyConditionExpression = "PK = :pk and begins_with(SK, :sk)",
            //    ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
            //    {
            //        { ":pk", new AttributeValue { S = studentKey } },
            //        { ":sk", new AttributeValue { S = "serie#" } }
            //    },
            //    Select = Select.COUNT
            //};

            List<Serie> response = await _dynamoDBContext
                .QueryAsync<Serie>(pk, QueryOperator.BeginsWith, new[] { "instructor#" }).GetRemainingAsync();

            Serie? highestSerie = response.OrderByDescending(s => s.Version).FirstOrDefault();

            if (highestSerie == null)
                serie.Version = 1;
            else
                serie.Version = highestSerie.Version + 1;

            string sortKey = $"instructor#{instructorKey}#serie#{serie.Version}";

            serie.PK = pk;
            serie.SK = sortKey;
            serie.GSI1PK = $"instructor#{instructorKey}";
            serie.GSI1SK = $"student#{studentKey}#serie#{serie.Version}";

            await _dynamoDBContext.SaveAsync(serie);

            return HttpResults.Created(null, serie);
        }
    }
}