using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Phoenix.Api.Models.Api;
using Phoenix.DataHandle.Main;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class LectureController : BaseController
    {
        private readonly ILogger<LectureController> _logger;
        private readonly LectureRepository _lectureRepository;
        private readonly Repository<Exercise> _exerciseRepository;
        private readonly Repository<Exam> _examRepository;
        private readonly Repository<AspNetUsers> _AspNetUserRepository;

        public LectureController(PhoenixContext phoenixContext, ILogger<LectureController> logger) : base(phoenixContext, logger)
        {
            this._logger = logger;
            this._lectureRepository = new LectureRepository(phoenixContext);
            this._lectureRepository.Include(a => a.Course, a => a.Classroom);
            this._exerciseRepository = new Repository<Exercise>(phoenixContext);
            this._examRepository = new Repository<Exam>(phoenixContext);
            this._AspNetUserRepository = new Repository<AspNetUsers>(phoenixContext);
        }

        [HttpGet]
        public async Task<IEnumerable<ILecture>> Get()
        {
            this._logger.LogInformation("Api -> Lecture -> Get");

            IQueryable<Lecture> lectures = this._lectureRepository.Find();

            return await lectures.Select(lecture => new LectureApi
            {
                id = lecture.Id,
                Status = lecture.Status,
                StartDateTime = lecture.StartDateTime,
                EndDateTime = lecture.EndDateTime,
                Info = lecture.Info,
                Course = new CourseApi
                {
                    id = lecture.Course.Id,
                    Name = lecture.Course.Name,
                    SubCourse = lecture.Course.SubCourse,
                    Level = lecture.Course.Level,
                    Group = lecture.Course.Group,
                    Info = lecture.Course.Info,
                    FirstDate = lecture.Course.FirstDate,
                    LastDate = lecture.Course.LastDate,
                },
                Classroom = lecture.Classroom != null
                    ? new ClassroomApi
                    {
                        id = lecture.Classroom.Id,
                        Name = lecture.Classroom.Name,
                        Info = lecture.Classroom.Info
                    }
                    : null,
                Exam = lecture.Exam != null
                    ? new ExamApi
                    {
                        id = lecture.Exam.Id,
                        Name = lecture.Exam.Name,
                        Comments = lecture.Exam.Comments,
                    }
                    : null,
            }).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ILecture> Get(int id)
        {
            this._logger.LogInformation($"Api -> Lecture -> Get{id}");

            Lecture lecture = await this._lectureRepository.Find(id);

            return new LectureApi
            {
                id = lecture.Id,
                Status = lecture.Status,
                StartDateTime = lecture.StartDateTime,
                EndDateTime = lecture.EndDateTime,
                Info = lecture.Info,
                Course = new CourseApi
                {
                    id = lecture.Course.Id,
                    Name = lecture.Course.Name,
                    SubCourse = lecture.Course.SubCourse,
                    Level = lecture.Course.Level,
                    Group = lecture.Course.Group,
                    Info = lecture.Course.Info,
                    FirstDate = lecture.Course.FirstDate,
                    LastDate = lecture.Course.LastDate,
                },
                Classroom = lecture.Classroom != null
                    ? new ClassroomApi
                    {
                        id = lecture.Classroom.Id,
                        Name = lecture.Classroom.Name,
                        Info = lecture.Classroom.Info
                    }
                    : null,
                Exam = lecture.Exam != null
                    ? new ExamApi
                    {
                        id = lecture.Exam.Id,
                        Name = lecture.Exam.Name,
                        Comments = lecture.Exam.Comments,
                    }
                    : null,
                Exercises = lecture.Exercise.Select(a => new ExerciseApi
                {
                    id = a.Id,
                    Name = a.Name,
                }).ToList(),
            };
        }

        [HttpGet("{id}/Exercise")]
        public async Task<IEnumerable<ExerciseApi>> GetExercises(int id)
        {
            this._logger.LogInformation($"Api -> Lecture -> Get -> {id} -> Exercises");

            IQueryable<Exercise> exercises = this._exerciseRepository.Find().Where(a => a.LectureId == id);

            return await exercises.Select(exercise => new ExerciseApi
            {
                id = exercise.Id,
                Lecture = new LectureApi
                {
                    id = exercise.Lecture.Id
                },
                Name = exercise.Name,
                Page = exercise.Page,
                Book = new BookApi
                {
                    id = exercise.Book.Id,
                    Name = exercise.Book.Name,
                    Publisher = exercise.Book.Publisher,
                    Info = exercise.Book.Info
                },
            }).ToListAsync();
        }

        [HttpGet("{id}/Exam")]
        public async Task<IEnumerable<ExamApi>> GetExam(int id)
        {
            this._logger.LogInformation($"Api -> Lecture -> Get -> {id} -> Exams");

            IQueryable<Exam> exams = this._examRepository.Find().Where(a => a.LectureId == id);

            return await exams.Select(exam => new ExamApi
            {
                id = exam.Id,
                Name = exam.Name,
                Comments = exam.Comments,
                Lecture = new LectureApi
                {
                    id = exam.Lecture.Id
                },
                Materials = exam.Material.Select(material => new MaterialApi
                {
                    id = material.Id,
                    Chapter = material.Chapter,
                    Section = material.Section,
                    Comments = material.Comments,
                    Book = material.Book != null
                        ? new BookApi
                        {
                            id = material.Book.Id,
                            Name = material.Book.Name,
                            Publisher = material.Book.Publisher,
                            Info = material.Book.Info
                        }
                        : null
                }).ToList()
            }).ToListAsync();
        }

        [HttpGet("{id}/Student")]
        public async Task<IEnumerable<UserApi>> GetStudent(int id)
        {
            this._logger.LogInformation($"Api -> Lecture -> Get -> {id} -> Students");

            IQueryable<AspNetUsers> users = this._AspNetUserRepository.Find().Where(a => a.StudentCourse.Any(b => b.Course.Lecture.Any(c => c.Id == id)));

            return await users.Select(user => new UserApi
            {
                id = user.Id,
                LastName = user.User.LastName,
                FirstName = user.User.FirstName,
                FullName = user.User.FullName,
                AspNetUser = new AspNetUserApi
                {
                    id = user.Id,
                },
                StudentCourses = user.StudentCourse.Select(a => new StudentCourse
                {
                    StudentId = a.StudentId,
                    CourseId = a.CourseId,
                    Grade = a.Grade
                })
            }).ToListAsync();
        }

        [HttpPost]
        public async Task<LectureApi> Post([FromBody] LectureApi lectureApi)
        {
            this._logger.LogInformation("Api -> Lecture -> Post");

            if (lectureApi == null)
                throw new ArgumentNullException(nameof(lectureApi));

            Lecture lecture = new Lecture
            {
                CourseId = lectureApi.Course.id,
                ClassroomId = lectureApi.Classroom.id,
                StartDateTime = lectureApi.StartDateTime,
                EndDateTime = lectureApi.EndDateTime,
                CreatedBy = LectureCreatedBy.Manual,
                Status = LectureStatus.Scheduled,
                Info = lectureApi.Info,
                Exam = lectureApi.Exam != null ? new Exam
                {
                    Name = lectureApi.Exam.Name,
                    Comments = lectureApi.Exam.Comments
                } : null,
                Attendance = new List<Attendance>(),
                Exercise = new List<Exercise>(),
                ScheduleId = null
            };

            lecture = await this._lectureRepository.Create(lecture);

            lecture = await this._lectureRepository.Find(lecture.Id);

            return new LectureApi
            {
                id = lecture.Id,
                Status = lecture.Status,
                StartDateTime = lecture.StartDateTime,
                EndDateTime = lecture.EndDateTime,
                Info = lecture.Info,
                Course = new CourseApi
                {
                    id = lecture.Course.Id,
                    Name = lecture.Course.Name,
                    SubCourse = lecture.Course.SubCourse,
                    Level = lecture.Course.Level,
                    Group = lecture.Course.Group,
                    Info = lecture.Course.Info,
                    FirstDate = lecture.Course.FirstDate,
                    LastDate = lecture.Course.LastDate,
                },
                Classroom = lecture.Classroom != null
                    ? new ClassroomApi
                    {
                        id = lecture.Classroom.Id,
                        Name = lecture.Classroom.Name,
                        Info = lecture.Classroom.Info
                    }
                    : null,
                Exam = lecture.Exam != null
                    ? new ExamApi
                    {
                        id = lecture.Exam.Id,
                        Name = lecture.Exam.Name,
                        Comments = lecture.Exam.Comments,
                    }
                    : null,
                Exercises = lecture.Exercise.Select(a => new ExerciseApi
                {
                    id = a.Id,
                    Name = a.Name,
                }).ToList(),
            };
        }
    }
}
