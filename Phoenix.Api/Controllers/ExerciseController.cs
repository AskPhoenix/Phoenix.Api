using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Phoenix.Api.Models.Api;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ExerciseController : BaseController
    {
        private readonly ILogger<ExerciseController> _logger;
        private readonly ExerciseRepository _exerciseRepository;

        public ExerciseController(PhoenixContext phoenixContext, ILogger<ExerciseController> logger) : base(phoenixContext, logger)
        {
            this._logger = logger;
            this._exerciseRepository = new ExerciseRepository(phoenixContext);
            this._exerciseRepository.include(a => a.Lecture);
        }

        [HttpGet("{id}")]
        public async Task<IExercise> Get(int id)
        {
            this._logger.LogInformation($"Api -> Exercise -> Get -> {id}");

            Exercise exercise = await this._exerciseRepository.find(id);

            return new ExerciseApi
            {
                id = exercise.Id,
                Name = exercise.Name,
                Page = exercise.Page,
                Comments = exercise.Comments,
                Book = new BookApi
                {
                    id = exercise.Book.Id,
                    Name = exercise.Book.Name,
                    Publisher = exercise.Book.Publisher,
                    Info = exercise.Book.Info
                },
                Lecture = new LectureApi
                {
                    id = exercise.Lecture.Id,
                    StartDateTime = exercise.Lecture.StartDateTime,
                    EndDateTime = exercise.Lecture.EndDateTime,
                    Status = exercise.Lecture.Status,
                    Info = exercise.Lecture.Info,
                    Course = new CourseApi
                    {
                        id = exercise.Lecture.Course.Id
                    },
                    Classroom = exercise.Lecture.Classroom != null ? new ClassroomApi
                    {
                        id = exercise.Lecture.Classroom.Id
                    } : null
                }
            };
        }

        [HttpPost]
        public async Task<ExerciseApi> Post([FromBody] ExerciseApi exerciseApi)
        {
            this._logger.LogInformation("Api -> Exercise -> Post");

            if (exerciseApi == null)
                throw new ArgumentNullException(nameof(exerciseApi));

            Exercise exercise = new Exercise
            {
                Name = exerciseApi.Name,
                Page = exerciseApi.Page,
                Comments = exerciseApi.Comments,
                LectureId = exerciseApi.Lecture.id,
                BookId = exerciseApi.Book.id,
            };

            exercise = this._exerciseRepository.create(exercise);

            exercise = await this._exerciseRepository.find(exercise.Id);

            return new ExerciseApi
            {
                id = exercise.Id,
                Name = exercise.Name,
                Page = exercise.Page,
                Comments = exercise.Comments,
                Book = exercise.Book != null
                    ? new BookApi
                    {
                        id = exercise.Book.Id,
                        Name = exercise.Book.Name,
                        Publisher = exercise.Book.Publisher,
                        Info = exercise.Book.Info
                    }
                    : null,
                Lecture = exercise.Lecture != null
                    ? new LectureApi
                    {
                        id = exercise.Lecture.Id,
                        StartDateTime = exercise.Lecture.StartDateTime,
                        EndDateTime = exercise.Lecture.EndDateTime,
                        Status = exercise.Lecture.Status,
                        Info = exercise.Lecture.Info,
                        Course = exercise.Lecture.Course != null
                            ? new CourseApi
                            {
                                id = exercise.Lecture.Course.Id
                            }
                            : null,
                        Classroom = exercise.Lecture.Classroom != null
                            ? new ClassroomApi
                            {
                                id = exercise.Lecture.Classroom.Id
                            }
                            : null
                    }
                    : null
            };
        }

        [HttpPut("{id}")]
        public async Task<ExerciseApi> Put(int id, [FromBody] ExerciseApi exerciseApi)
        {
            this._logger.LogInformation("Api -> Exercise -> Put -> {id}");

            if (exerciseApi == null)
                throw new ArgumentNullException(nameof(exerciseApi));

            Exercise exercise = new Exercise
            {
                Id = id,
                Name = exerciseApi.Name,
                Page = exerciseApi.Page,
                Comments = exerciseApi.Comments,
                LectureId = exerciseApi.Lecture.id,
                BookId = exerciseApi.Book.id,
            };

            exercise = this._exerciseRepository.update(exercise);

            exercise = await this._exerciseRepository.find(exercise.Id);

            return new ExerciseApi
            {
                id = exercise.Id,
                Name = exercise.Name,
                Page = exercise.Page,
                Comments = exercise.Comments,
                Book = exercise.Book != null
                    ? new BookApi
                    {
                        id = exercise.Book.Id,
                        Name = exercise.Book.Name,
                        Publisher = exercise.Book.Publisher,
                        Info = exercise.Book.Info
                    }
                    : null,
                Lecture = exercise.Lecture != null
                    ? new LectureApi
                    {
                        id = exercise.Lecture.Id,
                        StartDateTime = exercise.Lecture.StartDateTime,
                        EndDateTime = exercise.Lecture.EndDateTime,
                        Status = exercise.Lecture.Status,
                        Info = exercise.Lecture.Info,
                        Course = exercise.Lecture.Course != null
                            ? new CourseApi
                            {
                                id = exercise.Lecture.Course.Id
                            }
                            : null,
                        Classroom = exercise.Lecture.Classroom != null
                            ? new ClassroomApi
                            {
                                id = exercise.Lecture.Classroom.Id
                            }
                            : null
                    }
                    : null
            };
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            this._logger.LogInformation($"Api -> Exercise -> Delete -> {id}");

            this._exerciseRepository.delete(id);
        }


        [HttpGet("{id}/StudentExercise")]
        public async Task<IEnumerable<StudentExerciseApi>> GetStudentExercises(int id)
        {
            this._logger.LogInformation($"Api -> Exercise -> {id} -> StudentExercises");

            IQueryable<StudentExercise> studentExercises = this._exerciseRepository.FindStudentExercises(id);

            return await studentExercises.Select(studentExercise => new StudentExerciseApi
            {
                Grade = studentExercise.Grade,
                User = studentExercise.Student != null ? new UserApi
                {
                    id = studentExercise.Student.AspNetUserId,
                    FirstName = studentExercise.Student.FirstName,
                    LastName = studentExercise.Student.LastName,
                    FullName = studentExercise.Student.FullName,
                    AspNetUser = new AspNetUserApi
                    {
                        id = studentExercise.Student.AspNetUser.Id,
                        UserName = studentExercise.Student.AspNetUser.UserName,
                        Email = studentExercise.Student.AspNetUser.Email,
                        PhoneNumber = studentExercise.Student.AspNetUser.PhoneNumber
                    },
                } : null,
            }).ToListAsync();
        }


        [HttpPost("{id}/StudentExercise")]
        public async Task<StudentExerciseApi> PostStudentExercise(int id, [FromBody] StudentExerciseApi studentExerciseApi)
        {
            this._logger.LogInformation($"Api -> Exercise -> {id} -> StudentExercise -> Post");

            if (studentExerciseApi == null)
                throw new ArgumentNullException(nameof(studentExerciseApi));

            StudentExercise studentExercise = new StudentExercise
            {
                ExerciseId = id,
                Grade = studentExerciseApi.Grade,
                StudentId = studentExerciseApi.User.id
            };

            var exercise = await this._exerciseRepository.find(id);

            if (exercise.StudentExercise.Any(a => a.StudentId == studentExercise.StudentId))
            {
                var x = exercise.StudentExercise.Single(a => a.StudentId == studentExercise.StudentId);
                x.Grade = studentExercise.Grade;
            }
            else
            {
                exercise.StudentExercise.Add(studentExercise);
            }

            exercise = this._exerciseRepository.update(exercise);
            studentExercise = exercise.StudentExercise.SingleOrDefault(a => a.StudentId == studentExercise.StudentId);

            return new StudentExerciseApi
            {
                Grade = studentExercise.Grade,
                User = new UserApi
                {
                    id = studentExercise.StudentId,
                },
                Exercise = new ExerciseApi
                {
                    id = studentExercise.ExerciseId
                }
            };

        }

    }
}
