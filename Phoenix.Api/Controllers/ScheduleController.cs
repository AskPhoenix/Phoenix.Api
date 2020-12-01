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
    public class ScheduleController : BaseController
    {
        private readonly ILogger<ScheduleController> _logger;
        private readonly Repository<Schedule> _scheduleRepository;

        public ScheduleController(PhoenixContext phoenixContext, ILogger<ScheduleController> logger) : base(phoenixContext, logger)
        {
            this._logger = logger;
            this._scheduleRepository = new Repository<Schedule>(phoenixContext);
        }

        [HttpGet]
        public async Task<IEnumerable<ISchedule>> Get()
        {
            this._logger.LogInformation("Api -> Schedule -> Get");

            IQueryable<Schedule> schedules = this._scheduleRepository.Find();
            schedules = schedules.Where(a => a.Course.TeacherCourse.Any(b => b.TeacherId == this.userId));

            return await schedules.Select(schedule => new ScheduleApi
            {
                id = schedule.Id,
                DayOfWeek = schedule.DayOfWeek,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                Course = new CourseApi
                {
                    id = schedule.Course.Id,
                    Name = schedule.Course.Name,
                    SubCourse = schedule.Course.SubCourse,
                    Level = schedule.Course.Level,
                    Group = schedule.Course.Group,
                    Info = schedule.Course.Info,
                    FirstDate = schedule.Course.FirstDate,
                    LastDate = schedule.Course.LastDate,
                },
                Classroom = new ClassroomApi
                {
                    id = schedule.Classroom.Id,
                    Name = schedule.Classroom.Name,
                    Info = schedule.Classroom.Info
                },
            }).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ISchedule> Get(int id)
        {
            this._logger.LogInformation($"Api -> Schedule -> Get{id}");

            Schedule schedule = await this._scheduleRepository.Find(id);

            return new ScheduleApi
            {
                id = schedule.Id,
                DayOfWeek = schedule.DayOfWeek,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                Course = new CourseApi
                {
                    id = schedule.Course.Id,
                    Name = schedule.Course.Name,
                    SubCourse = schedule.Course.SubCourse,
                    Level = schedule.Course.Level,
                    Group = schedule.Course.Group,
                    Info = schedule.Course.Info,
                    FirstDate = schedule.Course.FirstDate,
                    LastDate = schedule.Course.LastDate,
                },
                Classroom = new ClassroomApi
                {
                    id = schedule.Classroom.Id,
                    Name = schedule.Classroom.Name,
                    Info = schedule.Classroom.Info
                },
            };
        }

    }
}
