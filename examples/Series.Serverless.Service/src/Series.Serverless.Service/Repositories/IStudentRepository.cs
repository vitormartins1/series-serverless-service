using Series.Serverless.Service.Entities;

namespace Series.Serverless.Service.Repositories
{
    public interface IStudentRepository
    {
        Task<bool> CreateAsync(Student student);

        Task<bool> DeleteAsync(Student student);

        Task<IList<Student>> GetStudentsAsync(int limit = 10);

        Task<Student?> GetStudentAsync(string pk);

        Task<bool> UpdateAsync(Student student);
    }
}