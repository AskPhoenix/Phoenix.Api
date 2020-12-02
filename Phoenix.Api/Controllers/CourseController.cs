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
    public class CourseController : BaseController
    {
        private readonly ILogger<CourseController> _logger;
        private readonly Repository<Course> _courseRepository;
        private readonly LectureRepository _lectureRepository;
        private readonly Repository<Schedule> _scheduleRepository;
        private readonly Repository<Book> _bookRepository;

        public CourseController(PhoenixContext phoenixContext, ILogger<CourseController> logger) : base(phoenixContext, logger)
        {
            this._logger = logger;
            this._courseRepository = new Repository<Course>(phoenixContext);
            this._lectureRepository = new LectureRepository(phoenixContext);
            this._scheduleRepository = new Repository<Schedule>(phoenixContext);
            this._bookRepository = new Repository<Book>(phoenixContext);
        }

        [HttpGet]
        public async Task<IEnumerable<CourseApi>> Get()
        {
            this._logger.LogInformation("Api -> Course -> Get");

            IQueryable<Course> courses = this._courseRepository.Find();
            courses = courses.Where(a => a.TeacherCourse.Any(b => b.TeacherId == this.userId));

            return await courses.Select(course => new CourseApi
            {
                id = course.Id,
                Name = course.Name,
                SubCourse = course.SubCourse,
                Group = course.Group,
                Level = course.Level,
                Info = course.Info,
                FirstDate = course.FirstDate,
                LastDate = course.LastDate,
                School = new SchoolApi
                {
                    id = course.School.Id,
                    Slug = course.School.Slug,
                    Name = course.School.Name,
                }
            }).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<CourseApi> Get(int id)
        {
            this._logger.LogInformation($"Api -> Course -> Get{id}");

            Course course  = await this._courseRepository.Find(id);

            return new CourseApi
            {
                id = course.Id,
                Name = course.Name,
                SubCourse = course.SubCourse,
                Group = course.Group,
                Level = course.Level,
                Info = course.Info,
                FirstDate = course.FirstDate,
                LastDate = course.LastDate,
                School = new SchoolApi
                {
                    id = course.School.Id,
                    Slug = course.School.Slug,
                    Name = course.School.Name,
                }
            };
        }

        [HttpGet("{id}/Lecture")]
        public async Task<IEnumerable<LectureApi>> GetLectures(int id)
        {
            this._logger.LogInformation($"Api -> Course -> Get -> {id} -> Lecture");

            IQueryable<Lecture> lectures = this._lectureRepository.Find().Where(a => a.CourseId == id);
            lectures = lectures.Where(a => a.Course.TeacherCourse.Any(b => b.TeacherId == this.userId));

            return await lectures.Select(lecture => new LectureApi
            {
                id = lecture.Id,
                Status = lecture.Status,
                StartDateTime = lecture.StartDateTime,
                EndDateTime = lecture.EndDateTime,
                Info = lecture.Info,
                Classroom = new ClassroomApi
                {
                    id = lecture.Classroom.Id,
                    Name = lecture.Classroom.Name,
                    Info = lecture.Classroom.Info
                },
                Exam = lecture.Exam != null ? new ExamApi
                {
                    id = lecture.Exam.Id,
                    Name = lecture.Exam.Name,
                    Comments = lecture.Exam.Comments,
                } : null,
                Exercises = lecture.Exercise.Select(a => new ExerciseApi
                {
                    id = a.Id,
                    Name = a.Name,
                }).ToList(),
            }).ToListAsync();
        }

        [HttpGet("{id}/Schedule")]
        public async Task<IEnumerable<ScheduleApi>> GetSchedules(int id)
        {
            this._logger.LogInformation($"Api -> Course -> Get -> {id} -> Schedule");

            IQueryable<Schedule> schedules = this._scheduleRepository.Find().Where(a => a.CourseId == id);
            schedules = schedules.Where(a => a.Course.TeacherCourse.Any(b => b.TeacherId == this.userId));

            return await schedules.Select(schedule => new ScheduleApi
            {
                id = schedule.Id,
                DayOfWeek = schedule.DayOfWeek,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                Info = schedule.Info,
                Classroom = new ClassroomApi
                {
                    id = schedule.Classroom.Id,
                    Name = schedule.Classroom.Name,
                    Info = schedule.Classroom.Info
                },
            }).ToListAsync();
        }

        [HttpGet("{id}/Book")]
        public async Task<IEnumerable<BookApi>> GetBooks(int id)
        {
            this._logger.LogInformation($"Api -> Course -> Get -> {id} -> Book");

            IQueryable<Book> books = this._bookRepository.Find().Where(a => a.CourseBook.Any(b => b.CourseId == id));
            books = books.Where(a => a.CourseBook.Any(c => c.Course.TeacherCourse.Any(b => b.TeacherId == this.userId)));

            return await books.Select(book => new BookApi
            {
                id = book.Id,
                Name = book.Name,
                Publisher = book.Publisher,
                Info = book.Info
            }).ToListAsync();
        }



    }
}
