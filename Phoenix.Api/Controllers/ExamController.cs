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
    public class ExamController : BaseController
    {
        private readonly ILogger<ExamController> _logger;
        private readonly ExamRepository _examRepository;

        public ExamController(PhoenixContext phoenixContext, ILogger<ExamController> logger) : base(phoenixContext, logger)
        {
            this._logger = logger;
            this._examRepository = new ExamRepository(phoenixContext);
            this._examRepository.Include(a => a.Lecture);
        }

        [HttpGet("{id}")]
        public async Task<IExam> Get(int id)
        {
            this._logger.LogInformation($"Api -> Exam -> Get -> {id}");

            Exam exam = await this._examRepository.Find(id);

            return new ExamApi
            {
                id = exam.Id,
                Name = exam.Name,
                Comments = exam.Comments,
                Materials = exam.Material != null
                    ? exam.Material.Select(material => new MaterialApi
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
                    : new List<MaterialApi>(),
                Lecture = exam.Lecture != null
                    ? new LectureApi
                    {
                        id = exam.Lecture.Id,
                        StartDateTime = exam.Lecture.StartDateTime,
                        EndDateTime = exam.Lecture.EndDateTime,
                        Status = exam.Lecture.Status,
                        Info = exam.Lecture.Info,
                        Course = exam.Lecture.Course != null
                            ? new CourseApi
                            {
                                id = exam.Lecture.Course.Id
                            }
                            : null,
                        Classroom = exam.Lecture.Classroom != null
                            ? new ClassroomApi
                            {
                                id = exam.Lecture.Classroom.Id
                            }
                            : null
                    }
                    : null,
            };
        }

        [HttpPost]
        public async Task<ExamApi> Post([FromBody] ExamApi examApi)
        {
            this._logger.LogInformation("Api -> Exam -> Post");

            if (examApi == null)
                throw new ArgumentNullException(nameof(examApi));

            Exam exam = new Exam
            {
                Name = examApi.Name,
                Comments = examApi.Comments,
                LectureId = examApi.Lecture.id
            };

            exam = this._examRepository.Create(exam);

            exam = await this._examRepository.Find(exam.Id);

            return new ExamApi
            {
                id = exam.Id,
                Name = exam.Name,
                Comments = exam.Comments,
                Materials = exam.Material != null
                    ? exam.Material.Select(material => new MaterialApi
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
                    : new List<MaterialApi>(),
                Lecture = exam.Lecture != null
                    ? new LectureApi
                    {
                        id = exam.Lecture.Id,
                        StartDateTime = exam.Lecture.StartDateTime,
                        EndDateTime = exam.Lecture.EndDateTime,
                        Status = exam.Lecture.Status,
                        Info = exam.Lecture.Info,
                        Course = exam.Lecture.Course != null
                            ? new CourseApi
                            {
                                id = exam.Lecture.Course.Id
                            }
                            : null,
                        Classroom = exam.Lecture.Classroom != null
                            ? new ClassroomApi
                            {
                                id = exam.Lecture.Classroom.Id
                            }
                            : null
                    }
                    : null,
            };
        }

        [HttpPut("{id}")]
        public async Task<ExamApi> Put(int id, [FromBody] ExamApi examApi)
        {
            this._logger.LogInformation($"Api -> Exam -> Put -> {id}");

            if (examApi == null)
                throw new ArgumentNullException(nameof(examApi));

            Exam exam = new Exam
            {
                Id = id,
                Name = examApi.Name,
                Comments = examApi.Comments,
                LectureId = examApi.Lecture.id
            };

            exam = this._examRepository.Update(exam);

            exam = await this._examRepository.Find(exam.Id);

            return new ExamApi
            {
                id = exam.Id,
                Name = exam.Name,
                Comments = exam.Comments,
                Materials = exam.Material != null
                    ? exam.Material.Select(material => new MaterialApi
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
                    : new List<MaterialApi>(),
                Lecture = exam.Lecture != null
                    ? new LectureApi
                    {
                        id = exam.Lecture.Id,
                        StartDateTime = exam.Lecture.StartDateTime,
                        EndDateTime = exam.Lecture.EndDateTime,
                        Status = exam.Lecture.Status,
                        Info = exam.Lecture.Info,
                        Course = exam.Lecture.Course != null
                            ? new CourseApi
                            {
                                id = exam.Lecture.Course.Id
                            }
                            : null,
                        Classroom = exam.Lecture.Classroom != null
                            ? new ClassroomApi
                            {
                                id = exam.Lecture.Classroom.Id
                            }
                            : null
                    }
                    : null,
            };
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            this._logger.LogInformation($"Api -> Exam -> Delete -> {id}");

            this._examRepository.Delete(id);
        }


        [HttpGet("{id}/StudentExam")]
        public async Task<IEnumerable<StudentExamApi>> GetStudentExams(int id)
        {
            this._logger.LogInformation($"Api -> Exam -> {id} -> StudentExams");

            IQueryable<StudentExam> studentExams = this._examRepository.FindStudentExams(id);

            return await studentExams.Select(studentExam => new StudentExamApi
            {
                Grade = studentExam.Grade,
                User = studentExam.Student != null ? new UserApi
                {
                    id = studentExam.Student.Id,
                    FirstName = studentExam.Student.User.FirstName,
                    LastName = studentExam.Student.User.LastName,
                    FullName = studentExam.Student.User.FullName,
                    AspNetUser = new AspNetUserApi
                    {
                        id = studentExam.Student.Id,
                        UserName = studentExam.Student.UserName,
                        Email = studentExam.Student.Email,
                        PhoneNumber = studentExam.Student.PhoneNumber
                    },
                } : null, 
            }).ToListAsync();
        }

        [HttpPost("{id}/StudentExam")]
        public async Task<StudentExamApi> PostStudentExam(int id, [FromBody] StudentExamApi studentExamApi)
        {
            this._logger.LogInformation($"Api -> Exam -> {id} -> StudentExam -> Post");

            if (studentExamApi == null)
                throw new ArgumentNullException(nameof(studentExamApi));

            StudentExam studentExam = new StudentExam
            {
                ExamId = id,
                Grade = studentExamApi.Grade,
                StudentId = studentExamApi.AspNetUser.id
            };

            var exam = await this._examRepository.Find(id);

            if (exam.StudentExam.Any(a => a.StudentId == studentExam.StudentId))
            {
                var x = exam.StudentExam.Single(a => a.StudentId == studentExam.StudentId);
                x.Grade = studentExam.Grade;
            }
            else
            {
                exam.StudentExam.Add(studentExam);
            }

            exam = this._examRepository.Update(exam);
            studentExam = exam.StudentExam.SingleOrDefault(a => a.StudentId == studentExam.StudentId);

            return new StudentExamApi
            {
                Grade = studentExam.Grade,
                User = new UserApi
                {
                    id = studentExam.StudentId,
                },
                Exam = new ExamApi
                {
                    id = studentExam.ExamId
                }
            };

        }


    }
}
