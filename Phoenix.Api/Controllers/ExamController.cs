using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phoenix.DataHandle.Api;
using Phoenix.DataHandle.Api.Models;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public class ExamController : ApplicationController
    {
        private readonly ExamRepository _examRepository;

        public ExamController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<ExamController> logger)
            : base(phoenixContext, userManager, logger)
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

        // TODO: Grades?

        [HttpGet("{id}")]
        public async Task<ExamApi?> GetAsync(int id)
        {
            _logger.LogInformation("Api -> Exam -> Get -> {id}", id);

            var exam = await this.FindAsync(id);
            if (exam is null)
                return null;

            return new ExamApi(exam);
        }

        [HttpPost]
        public async Task<ExamApi?> PostAsync([FromBody] ExamApi examApi)
        {
            _logger.LogInformation("Api -> Exam -> Post");

            if (examApi is null)
            {
                _logger.LogError("Argument {arg} cannot be null.", nameof(examApi));
                return null;
            }

            var exam = await _examRepository.CreateAsync(examApi.ToExam());
            return new ExamApi(exam);
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

            var exam = await this.FindAsync(id);
            if (exam is null)
                return null;

            exam = await _examRepository.UpdateAsync(examApi.ToExam(exam));
            return new ExamApi(exam);
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
