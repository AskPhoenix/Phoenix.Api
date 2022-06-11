﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phoenix.DataHandle.Api.Models.Main;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ScheduleController : ApplicationController
    {
        private readonly ScheduleRepository _scheduleRepository;

        public ScheduleController(
            ILogger<ScheduleController> logger,
            ApplicationUserManager userManager,
            PhoenixContext phoenixContext)
            : base(logger, userManager)
        {
            _scheduleRepository = new(phoenixContext);
        }

        [HttpGet]
        public IEnumerable<ScheduleApi>? Get()
        {
            _logger.LogInformation("Api -> Schedule -> Get");

            if (!this.CheckUserAuth())
                return null;

            return this.AppUser?.User.Courses
                .SelectMany(c => c.Schedules.Select(s => new ScheduleApi(s, include: true)));
        }

        [HttpGet("{id}")]
        public async Task<ScheduleApi?> GetAsync(int id)
        {
            _logger.LogInformation("Api -> Schedule -> Get {id}", id);

            if (!this.CheckUserAuth())
                return null;

            var schedule = await _scheduleRepository.FindPrimaryAsync(id);
            if (schedule is null)
                return null;

            if (!schedule.Course.Users.Any(u => u.AspNetUserId == this.AppUser!.Id))
            {
                _logger.LogError("User with id {UserId} " +
                    "is not authorized to access schedule with id {id}", id);
                return null;
            }

            return new ScheduleApi(schedule, include: true);
        }
    }
}
