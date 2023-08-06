using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using Series.Serverless.Service.Entities;
using Series.Serverless.Service.Repositories;

namespace Series.Serverless.Service.Controllers;

[Route("api/[controller]")]
[Produces("application/json")]
public class StudentsController : ControllerBase
{
    private readonly ILogger<StudentsController> logger;
    private readonly IStudentRepository studentRepository;

    public StudentsController(ILogger<StudentsController> logger, IStudentRepository studentRepository)
    {
        this.logger = logger;
        this.studentRepository = studentRepository;
    }

    // GET api/students
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Student>>> Get([FromQuery] int limit = 10)
    {
        if (limit <= 0 || limit > 100) return BadRequest("The limit should been between [1-100]");

        return Ok(await studentRepository.GetStudentsAsync(limit));
    }

    // GET api/students/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Student>> Get(string pk)
    {
        var result = await studentRepository.GetStudentAsync(pk);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    // POST api/students
    [HttpPost]
    public async Task<ActionResult<Student>> Post([FromBody] Student student)
    {
        if (student == null) return ValidationProblem("Invalid input! Student not informed");

        var result = await studentRepository.CreateAsync(student);

        if (result)
        {
            return CreatedAtAction(
                nameof(Get),
                new { pk = student.PK, sk = student.SK },
                student);
        }
        else
        {
            return BadRequest("Fail to persist");
        }

    }

    // PUT api/students/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string pk, [FromBody] Student student )
    {
        //if (id == Guid.Empty || book == null) return ValidationProblem("Invalid request payload");

        //// Retrieve the students.
        //var bookRetrieved = await studentRepository.GetStudentAsync(id);

        //if (bookRetrieved == null)
        //{
        //    var errorMsg = $"Invalid input! No book found with id:{id}";
        //    return NotFound(errorMsg);
        //}

        //book.Id = bookRetrieved.Id;

        //await s.UpdateAsync(book);
        return Ok();
    }

    // DELETE api/students/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string pk)
    {
        if (string.IsNullOrEmpty(pk)) return ValidationProblem("Invalid request payload");

        Student? studentRetrieved = await studentRepository.GetStudentAsync(pk);

        if (studentRetrieved == null)
        {
            string errorMsg = $"Invalid input! No book found with pk:{pk}";
            return NotFound(errorMsg);
        }

        await studentRepository.DeleteAsync(studentRetrieved);
        return Ok();
    }
}
