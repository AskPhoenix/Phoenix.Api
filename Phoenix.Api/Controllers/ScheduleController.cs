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

namespace Phoenix.Api.Controllers
{
    [Route("api/[controller]")]
    public class ScheduleController : BaseController
    {
        private readonly ILogger<ScheduleController> _logger;
        private readonly Repository<Schedule> _scheduleRepository;

        public ScheduleController(PhoenixContext phoenixContext, ILogger<ScheduleController> logger)
        {
            this._logger = logger;
            this._scheduleRepository = new Repository<Schedule>(phoenixContext);
        }

        [HttpGet]
        public async Task<IEnumerable<ISchedule>> Get()
        {
            this._logger.LogInformation("Api -> Schedule -> Get");

            IQueryable<Schedule> schedules = this._scheduleRepository.find();

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
                    Level = schedule.Course.Level,
                    Group = schedule.Course.Group,
                    Info = schedule.Course.Info
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

            Schedule schedule = await this._scheduleRepository.find(id);

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
                    Level = schedule.Course.Level,
                    Group = schedule.Course.Group,
                    Info = schedule.Course.Info
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
