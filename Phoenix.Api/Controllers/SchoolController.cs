using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phoenix.DataHandle.Api.Models;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/[controller]")]
    public class SchoolController : ApplicationController
    {
        private readonly SchoolRepository _schoolRepository;

        public SchoolController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<SchoolController> logger)
            : base(phoenixContext, userManager, logger)
        {
            _schoolRepository = new(phoenixContext, nonObviatedOnly: true);
        }

        private async Task<School?> FindAsync(int id)
        {
            if (!this.CheckUserAuth())
                return null;

            var school = await _schoolRepository.FindPrimaryAsync(id);
            if (school is null)
            {
                _logger.LogError("No school found with id {id}", id);
                return null;
            }

            if (!school.Users.Any(u => u.AspNetUserId == this.AppUser!.Id))
            {
                _logger.LogError("User with id {UserId} " +
                    "is not authorized to access school with id {id}", id);
                return null;
            }

            return school;
        }

        [HttpGet]
        public IEnumerable<SchoolApi>? Get()
        {
            _logger.LogInformation("Api -> School -> Get");

            if (!this.CheckUserAuth())
                return null;

            return this.PhoenixUser?.Schools
                .Select(s => new SchoolApi(s));
        }

        [HttpGet("{id}")]
        public async Task<SchoolApi?> GetAsync(int id)
        {
            _logger.LogInformation("Api -> School -> Get {id}", id);

            var school = await this.FindAsync(id);
            if (school is null)
                return null;

            return new SchoolApi(school);
        }

        [HttpGet("{id}/Classrooms")]
        public async Task<IEnumerable<ClassroomApi>?> GetClassroomsAsync(int id)
        {
            _logger.LogInformation("Api -> School -> {id} -> Classrooms", id);

            var school = await this.FindAsync(id);
            if (school is null)
                return null;

            return school.Classrooms.Select(c => new ClassroomApi(c));
        }

        [HttpGet("{id}/Courses")]
        public async Task<IEnumerable<CourseApi>?> GetCoursesAsync(int id)
        {
            _logger.LogInformation("Api -> School -> {id} -> Courses", id);
            
            var school = await this.FindAsync(id);
            if (school is null)
                return null;

            return school.Courses.Select(c => new CourseApi(c));
        }
    }
}
