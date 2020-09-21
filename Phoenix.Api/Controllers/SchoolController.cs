using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Phoenix.Api.Models.Api;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class SchoolController : BaseController
    {
        private readonly ILogger<SchoolController> _logger;
        private readonly SchoolRepository _schoolRepository;

        public SchoolController(PhoenixContext phoenixContext, ILogger<SchoolController> logger) : base(phoenixContext, logger)
        {
            this._logger = logger;
            this._schoolRepository = new SchoolRepository(phoenixContext);
        }

        [HttpGet]
        public async Task<IEnumerable<SchoolApi>> Get()
        {
            this._logger.LogInformation("Api -> School -> Get");

            IQueryable<School> schools = this._schoolRepository.find();
            schools = schools.Where(a => a.Course.Any(b => b.TeacherCourse.Any(c => c.TeacherId == this.userId)));

            return await schools.Select(school => new SchoolApi
            {
                id = school.Id,
                Name = school.Name,
                Slug = school.Slug,
                AddressLine = school.AddressLine,
                City = school.City,
                FacebookPageId = school.FacebookPageId,
                Info = school.Info,
            }).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<SchoolApi> Get(int id)
        {
            this._logger.LogInformation($"Api -> School -> Get{id}");

            School school = await this._schoolRepository.find(id);

            return new SchoolApi
            {
                id = school.Id,
                Name = school.Name,
                Slug = school.Slug,
                AddressLine = school.AddressLine,
                City = school.City,
                FacebookPageId = school.FacebookPageId,
                Info = school.Info,
            };
        }

        [HttpPost]
        public Task<SchoolApi> Post([FromBody] SchoolApi schoolApi)
        {
            this._logger.LogInformation("Api -> School -> Post");

            if (schoolApi == null)
                throw new ArgumentNullException(nameof(schoolApi));

            return Task.FromResult(schoolApi);
        }

        [HttpPut("{id}")]
        public Task<SchoolApi> Put(int id, [FromBody] SchoolApi schoolApi)
        {
            this._logger.LogInformation("Api -> School -> Put -> {id}");

            if (schoolApi == null)
                throw new ArgumentNullException(nameof(schoolApi));

            return Task.FromResult(schoolApi);
        }

        [HttpDelete("{id}")]
        public void Delete(int id) 
        { 
            this._logger.LogInformation($"Api -> School -> Get{id}");
        }


        [HttpGet("{id}/Classroom")]
        public async Task<IEnumerable<ClassroomApi>> GetClassrooms(int id)
        {
            this._logger.LogInformation($"Api -> School -> {id} -> Classrooms");

            IQueryable<Classroom> classrooms = this._schoolRepository.FindClassrooms(id);

            return await classrooms.Select(classroom => new ClassroomApi
            {
                id = classroom.Id,
                Name = classroom.Name,
                Info = classroom.Info,
            }).ToListAsync();
        }

        [HttpGet("{id}/Course")]
        public async Task<IEnumerable<CourseApi>> GetCourses(int id)
        {
            this._logger.LogInformation($"Api -> School -> {id} -> Courses");

            IQueryable<Course> courses = this._schoolRepository.FindCourses(id);

            return await courses.Select(course => new CourseApi
            {
                id = course.Id,
                Name = course.Name,
                Group = course.Group,
                SubCourse = course.SubCourse,
                Level = course.Level,
                Info = course.Info,
                FirstDate = course.FirstDate,
                LastDate = course.LastDate,
            }).ToListAsync();
        }


    }
}
