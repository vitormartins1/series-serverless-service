using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using SeriesAPI.Entities;
using SeriesAPI.Repository.Interface;

namespace SeriesAPI.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly IDynamoDBContext context;
        private readonly ILogger<StudentRepository> logger;

        public StudentRepository(IDynamoDBContext context, ILogger<StudentRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<bool> CreateAsync(Student student)
        {
            try
            {
                string key = $"student#{Guid.NewGuid()}";
                student.PK = key;
                student.SK = key;
                await context.SaveAsync(student);
                logger.LogInformation("Book {} is added", student.PK);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "fail to persist to DynamoDb Table");
                return false;
            }

            return true;
        }

        public Task<bool> DeleteAsync(Student student)
        {
            throw new NotImplementedException();
        }

        public async Task<Student?> GetStudentAsync(string pk)
        {
            try
            {
                Student student = await context.LoadAsync<Student>(pk, pk);
                return student;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "fail to Get student data from DynamoDb SeriesTable");
                return null;
            }
        }

        public Task<IList<Student>> GetStudentsAsync(int limit = 10)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(Student student)
        {
            throw new NotImplementedException();
        }
    }
}
