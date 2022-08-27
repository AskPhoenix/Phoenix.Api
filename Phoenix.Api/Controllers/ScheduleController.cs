using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phoenix.DataHandle.Api;
using Phoenix.DataHandle.Api.Models;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/[controller]")]
    public class ScheduleController : ApplicationController
    {
        private readonly ScheduleRepository _scheduleRepository;

        public ScheduleController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<ScheduleController> logger)
            : base(phoenixContext, userManager, logger)
        {
            _scheduleRepository = new(phoenixContext, nonObviatedOnly: true);
        }

        [HttpGet]
        public IEnumerable<ScheduleApi>? Get()
        {
            _logger.LogInformation("Api -> Schedule -> Get");

            if (!this.CheckUserAuth())
                return null;

            return this.PhoenixUser?.Courses
                .SelectMany(c => c.Schedules)
                .Select(s => new ScheduleApi(s));
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

            return new ScheduleApi(schedule);
        }
    }
}
