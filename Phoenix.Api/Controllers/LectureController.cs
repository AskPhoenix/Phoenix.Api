﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    [Route("api/[controller]")]
    public class LectureController : BaseController
    {
        private readonly ILogger<LectureController> _logger;
        private readonly Repository<Lecture> _lectureRepository;
        private readonly Repository<Exercise> _exerciseRepository;
        private readonly Repository<Exam> _examRepository;

        public LectureController(PhoenixContext phoenixContext, ILogger<LectureController> logger)
        {
            this._logger = logger;
            this._lectureRepository = new Repository<Lecture>(phoenixContext);
            this._exerciseRepository = new Repository<Exercise>(phoenixContext);
            this._examRepository = new Repository<Exam>(phoenixContext);
        }

        [HttpGet]
        public async Task<IEnumerable<ILecture>> Get()
        {
            this._logger.LogInformation("Api -> Lecture -> Get");

            IQueryable<Lecture> lectures = this._lectureRepository.find();

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
                    Level = lecture.Course.Level,
                    Group = lecture.Course.Group,
                    Info = lecture.Course.Info
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

            Lecture lecture = await this._lectureRepository.find(id);

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
                    Level = lecture.Course.Level,
                    Group = lecture.Course.Group,
                    Info = lecture.Course.Info
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

            IQueryable<Exercise> exercises = this._exerciseRepository.find().Where(a => a.LectureId == id);

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
                },
            }).ToListAsync();
        }

        [HttpGet("{id}/Exam")]
        public async Task<IEnumerable<ExamApi>> GetExam(int id)
        {
            this._logger.LogInformation($"Api -> Lecture -> Get -> {id} -> Exams");

            IQueryable<Exam> exams = this._examRepository.find().Where(a => a.LectureId == id);

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
                        }
                        : null
                }).ToList()
            }).ToListAsync();
        }


    }
}
