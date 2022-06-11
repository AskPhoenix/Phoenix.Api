using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phoenix.DataHandle.Api.Models.Main;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ExamController : ApplicationController
    {
        private readonly ExamRepository _examRepository;

        public ExamController(
            ILogger<ExamController> logger,
            ApplicationUserManager userManager,
            PhoenixContext phoenixContext)
            : base(logger, userManager)
        {
            _examRepository = new(phoenixContext);
        }

        private async Task<Exam?> FindAsync(int id)
        {
            if (!this.CheckUserAuth())
                return null;

            var exam = await _examRepository.FindPrimaryAsync(id);
            if (exam is null)
            {
                _logger.LogError("No exam found with id {id}", id);
                return null;
            }

            if (!exam.Lecture.Course.Users.Any(u => u.AspNetUserId == this.AppUser!.Id))
            {
                _logger.LogError("User with id {UserId} " +
                    "is not authorized to access exam with id {id}", id);
                return null;
            }

            return exam;
        }

        [HttpGet("{id}")]
        public async Task<ExamApi?> GetAsync(int id)
        {
            _logger.LogInformation("Api -> Exam -> Get -> {id}", id);

            var exam = await this.FindAsync(id);
            if (exam is null)
                return null;

            return new ExamApi(exam, include: true);
        }

        // TODO: Check if lectures are created for the exam.
        // TODO: Check if the list properties are affected. E.g. add material
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

            var oldExam = await this.FindAsync(id);
            if (oldExam is null)
                return null;

            var exam = await _examRepository.UpdateAsync((Exam)(IExam)examApi);
            return new ExamApi(exam, include: true);
        }

        [HttpDelete("{id}")]
        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Api -> Exam -> Delete -> {id}", id);

            var exam = await this.FindAsync(id);
            if (exam is null)
                return;

            await _examRepository.DeleteAsync(id);
        }
    }
}
