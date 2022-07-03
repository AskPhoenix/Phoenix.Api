using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phoenix.DataHandle.Api.Models.Main;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class CourseController : ApplicationController
    {
        private readonly CourseRepository _courseRepository;

        public CourseController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<CourseController> logger)
            : base(phoenixContext, userManager, logger)
        {
            _courseRepository = new(phoenixContext);
        }

        private async Task<Course?> FindAsync(int id)
        {
            if (!this.CheckUserAuth())
                return null;

            var course = await _courseRepository.FindPrimaryAsync(id);
            if (course is null)
            {
                _logger.LogError("No course found with id {id}", id);
                return null;
            }

            if (!course.Users.Any(u => u.AspNetUserId == this.AppUser!.Id))
            {
                _logger.LogError("User with id {UserId} " +
                    "is not authorized to access course with id {id}", id);
                return null;
            }

            return course;
        }

        [HttpGet]
        public IEnumerable<CourseApi>? Get()
        {
            _logger.LogInformation("Api -> Course -> Get");

            if (!this.CheckUserAuth())
                return null;

            // TODO: Check if lazy properties are loaded
            return this.PhoenixUser?.Courses
                .Select(c => new CourseApi(c, include: true));
        }

        [HttpGet("{id}")]
        public async Task<CourseApi?> GetAsync(int id)
        {
            _logger.LogInformation("Api -> Course -> Get {id}", id);

            var course = await this.FindAsync(id);
            if (course is null)
                return null;

            return new CourseApi(course, include: true);
        }

        [HttpGet("{id}/Lectures")]
        public async Task<IEnumerable<LectureApi>?> GetLecturesAsync(int id)
        {
            _logger.LogInformation("Api -> Course -> Get -> {id} -> Lectures", id);

            var course = await this.FindAsync(id);
            return course?.Lectures.Select(l => new LectureApi(l, include: true));
        }

        [HttpGet("{id}/Schedules")]
        public async Task<IEnumerable<ScheduleApi>?> GetSchedulesAsync(int id)
        {
            _logger.LogInformation("Api -> Course -> Get -> {id} -> Schedules", id);

            var course = await this.FindAsync(id);
            return course?.Schedules.Select(s => new ScheduleApi(s, include: true));
        }

        [HttpGet("{id}/Books")]
        public async Task<IEnumerable<BookApi>?> GetBooksAsync(int id)
        {
            _logger.LogInformation("Api -> Course -> Get -> {id} -> Books", id);

            var course = await this.FindAsync(id);
            return course?.Books.Select(b => new BookApi(b));
        }
    }
}
