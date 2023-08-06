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

        [LambdaFunction()]
        [HttpApi(LambdaHttpMethod.Get, "/")]
        public string Default()
        {
            var docs = @"Serviço de gerenciamento de versões de séries de exercícios entre instrutor e aluno.";
            return docs;
        }

        [LambdaFunction(ResourceName = "CreateStudentFunction")]
        [HttpApi(LambdaHttpMethod.Post, "/student")]
        public async Task<IHttpResult> CreateStudentAsync([FromBody] Student student, ILambdaContext context)
        {
            string key = $"student#{Guid.NewGuid().ToString()}";

            //var student = JsonSerializer.Deserialize<Student>(request.Body);

            student.PK = key;
            student.SK = key;
            await _dynamoDBContext.SaveAsync(student);

            return HttpResults.Created(null, student);
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
            instructor.IndexInstructor = key;

            await _dynamoDBContext.SaveAsync(instructor);

            return HttpResults.Created(null, instructor);
        }


        [LambdaFunction(ResourceName = "CreateSerieFunction")]
        [HttpApi(LambdaHttpMethod.Post, "/serie/{studentKey}")]
        public async Task<IHttpResult> CreateSerieAsync(
            [FromBody] Serie serie, string studentKey, ILambdaContext context)
        {
            string sortKey = $"serie#{Guid.NewGuid().ToString()}";
            string pk = $"student#{studentKey}";

            serie.PK = pk;
            serie.SK = sortKey;

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
                .QueryAsync<Serie>(pk, QueryOperator.BeginsWith, new[] { "serie#" }).GetRemainingAsync();

            int? highestVersion = response.OrderByDescending(s => s.Version).FirstOrDefault().Version;

            serie.Version = highestVersion + 1;

            await _dynamoDBContext.SaveAsync(serie);

            return HttpResults.Created(null, serie);
        }
    }
}