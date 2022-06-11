using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phoenix.DataHandle.Api.Models.Main;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ExamController : Controller
    {
        private readonly ILogger<ExamController> _logger;
        private readonly ExamRepository _examRepository;

        public ExamController(
            ILogger<ExamController> logger,
            PhoenixContext phoenixContext)
        {
            _logger = logger;
            _examRepository = new(phoenixContext);

            // TODO: Check if Include is ok
            _examRepository.Include(e => e.Lecture);
            _examRepository.Include(e => e.Grades);
            _examRepository.Include(e => e.Materials);
        }

        [HttpGet("{id}")]
        public async Task<ExamApi?> GetAsync(int id)
        {
            _logger.LogInformation("Api -> Exam -> Get -> {id}", id);

            var exam = await _examRepository.FindPrimaryAsync(id);
            if (exam is null)
                return null;

            return new ExamApi(exam, include: true);
        }

        // TODO: Check if the list properties are affected.
        // If not, a method that converts a ModelApi to ModelEntity will be needed inside each ModelApi
        [HttpPost]
        public async Task<ExamApi?> PostAsync([FromBody] ExamApi examApi)
        {
            _logger.LogInformation("Api -> Exam -> Post");

            if (examApi is null)
            {
                _logger.LogError("Argument {arg} cannot be null.", nameof(examApi));
                return null;
            }

            var exam = await _examRepository.CreateAsync((Exam)(IExam)examApi);
            return new ExamApi(exam, include: true);
        }

        [HttpPut("{id}")]
        public async Task<ExamApi?> PutAsync(int id, [FromBody] ExamApi examApi)
        {
            _logger.LogInformation("Api -> Exam -> Put -> {id}", id);

            if (examApi is null)
            {
                _logger.LogError("Argument {arg} cannot be null.", nameof(examApi));
                return null;
            }

            var exam = await _examRepository.UpdateAsync((Exam)(IExam)examApi);
            return new ExamApi(exam, include: true);
        }

        [HttpDelete("{id}")]
        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Api -> Exam -> Delete -> {id}", id);

            await _examRepository.DeleteAsync(id);
        }
    }
}
