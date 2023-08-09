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
        private readonly IAmazonDynamoDB _dynamoDBClient;
        private readonly IDynamoDBContext _dynamoDBContext;

        public Functions(
            IDynamoDBContext dynamoDBContext, 
            IAmazonDynamoDB dynamoDBClient)
        {
            _dynamoDBContext = dynamoDBContext;
            _dynamoDBClient = dynamoDBClient;
        }

        [LambdaFunction(ResourceName = "CreateStudent")]
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
        [HttpApi(LambdaHttpMethod.Get, "/serie/all/student/{studentKey}")]
        public async Task<IHttpResult> GetAllSeriesFromStudentAsync(string studentKey, ILambdaContext context)
        {
            List<Serie> series = await _dynamoDBContext
                .QueryAsync<Serie>($"student#{studentKey}", QueryOperator.BeginsWith, new[] { "instructor#" }).GetRemainingAsync();

            return HttpResults.Ok(series);
        }

        [LambdaFunction(ResourceName = "GetStudentByKey")]
        [HttpApi(LambdaHttpMethod.Get, "/student/{studentKey}")]
        public async Task<IHttpResult> GetStudentByKeyAsync(string studentKey, ILambdaContext context)
        {
            string key = $"student#{studentKey}";

            Student student = await _dynamoDBContext
                .LoadAsync<Student>(key, key);

            return HttpResults.Ok(student);
        }

        [LambdaFunction(ResourceName = "GetAllStudentsFromInstructor")]
        [HttpApi(LambdaHttpMethod.Get, "/student/instructor/{instructorKey}")]
        public async Task<IHttpResult> GetAllStudentsFromInstructorAsync(string instructorKey, ILambdaContext context)
        {
            var students = _dynamoDBContext.QueryAsync<Student>(
                $"instructor#{instructorKey}",
                QueryOperator.BeginsWith,
                new[] { "student#" },
                new DynamoDBOperationConfig
                {
                    IndexName = "GSI1"
                })
            .GetRemainingAsync()
            .Result
            .Where(s => !s.SK.Contains("#serie#"));

            return HttpResults.Ok(students);
        }

        [LambdaFunction(ResourceName = "GetCurrentSerieFromStudent")]
        [HttpApi(LambdaHttpMethod.Get, "/serie/current/student/{studentKey}")]
        public async Task<IHttpResult> GetCurrentSerieFromStudentAsync(string studentKey, ILambdaContext context)
        {
            var serie = _dynamoDBContext.QueryAsync<Serie>(
                $"student#{studentKey}",
                QueryOperator.BeginsWith,
                new[] { "instructor#" })
            .GetRemainingAsync()
            .Result
            .OrderByDescending(s => s.Version)
            .FirstOrDefault();

            return HttpResults.Ok(serie);
        }

        [LambdaFunction(ResourceName = "DeleteStudent")]
        [HttpApi(LambdaHttpMethod.Delete, "/student/{studentKey}")]
        public async Task<IHttpResult> DeleteStudentAsync(string studentKey, ILambdaContext context)
        {
            string pk = $"student#{studentKey}";
            string sk = $"student#{studentKey}";

            Student student = await _dynamoDBContext.LoadAsync<Student>(pk, sk);

            if (student != null)
            {
                await _dynamoDBContext.DeleteAsync<Student>(pk, sk);

                return HttpResults.NewResult(HttpStatusCode.NoContent);
            }

            return HttpResults.NotFound();
        }

        [LambdaFunction(ResourceName = "CreateInstructor")]
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
        [HttpApi(LambdaHttpMethod.Get, "/instructor/{instructorKey}")]
        public async Task<IHttpResult> GetInstructorByKeyAsync(string instructorKey, ILambdaContext context)
        {
            string key = $"instructor#{instructorKey}";

            Instructor instructor = await _dynamoDBContext
                .LoadAsync<Instructor>(key, key);

            return HttpResults.Ok(instructor);
        }

        [LambdaFunction(ResourceName = "GetSerieByVersion")]
        [HttpApi(LambdaHttpMethod.Get, "/serie/{serieVersion}/student/{studentKey}/instructor/{instructorKey}")]
        public async Task<IHttpResult> GetSerieByKeyAsync(int serieVersion, string studentKey, string instructorKey, ILambdaContext context)
        {
            List<Serie> series = await _dynamoDBContext.QueryAsync<Serie>(
                $"student#{studentKey}",
                QueryOperator.BeginsWith,
                new[] { $"instructor#{instructorKey}#serie#{serieVersion}" })
            .GetRemainingAsync();

            Serie? serie = series.FirstOrDefault();

            return HttpResults.Ok(serie);
        }

        [LambdaFunction(ResourceName = "GetAllSeriesFromStudentAndInstructor")]
        [HttpApi(LambdaHttpMethod.Get, "/serie/student/{studentKey}/instructor/{instructorKey}")]
        public async Task<IHttpResult> GetAllSeriesFromStudentAndInstructorAsync(string studentKey, string instructorKey, ILambdaContext context)
        {
            var series = await _dynamoDBContext.QueryAsync<Serie>(
                $"instructor#{instructorKey}",
                QueryOperator.BeginsWith,
                new[] { $"student#{studentKey}#serie#" },
                new DynamoDBOperationConfig
                {
                    IndexName = "GSI1"
                })
            .GetRemainingAsync();

            return HttpResults.Ok(series);
        }

        [LambdaFunction(ResourceName = "CreateSerie")]
        [HttpApi(LambdaHttpMethod.Post, "/serie/student/{studentKey}/instructor/{instructorKey}")]
        public async Task<IHttpResult> CreateSerieAsync(
            [FromBody] Serie serie, string studentKey, string instructorKey, ILambdaContext context)
        {
            string pk = $"student#{studentKey}";

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

        [LambdaFunction(ResourceName = "GetAllStudents")]
        [HttpApi(LambdaHttpMethod.Get, "/student")]
        public async Task<IHttpResult> GetAllStudentsAsync(ILambdaContext context)
        {
            List<Student> table = await _dynamoDBContext
                .ScanAsync<Student>(new List<ScanCondition>())
                .GetRemainingAsync();

            List<Student> students = table.Where(s => s.SK.Contains("student#")).ToList();

            return HttpResults.Ok(students);
        }

        [LambdaFunction(ResourceName = "GetAllInstructors")]
        [HttpApi(LambdaHttpMethod.Get, "/instructor")]
        public async Task<IHttpResult> GetAllInstructorsAsync(ILambdaContext context)
        {
            List<Instructor> table = await _dynamoDBContext
                .ScanAsync<Instructor>(new List<ScanCondition>())
                .GetRemainingAsync();

            List<Instructor> instructors = table.Where(s => s.PK.Contains("instructor#")).ToList();

            return HttpResults.Ok(instructors);
        }
    }
}