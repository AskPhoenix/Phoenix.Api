using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phoenix.DataHandle.Api.Models.Main;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/[controller]")]
    public class ExerciseController : ApplicationController
    {
        private readonly ExerciseRepository _exerciseRepository;

        public ExerciseController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<ExerciseController> logger)
            : base(phoenixContext, userManager, logger)
        {
            _exerciseRepository = new(phoenixContext);
        }

        private async Task<Exercise?> FindAsync(int id)
        {
            if (!this.CheckUserAuth())
                return null;

            var exercise = await _exerciseRepository.FindPrimaryAsync(id);
            if (exercise is null)
            {
                _logger.LogError("No exercise found with id {id}", id);
                return null;
            }

            if (!exercise.Lecture.Course.Users.Any(u => u.AspNetUserId == this.AppUser!.Id))
            {
                _logger.LogError("User with id {UserId} " +
                    "is not authorized to access exercise with id {id}", id);
                return null;
            }

            return exercise;
        }

        [HttpGet("{id}")]
        public async Task<ExerciseApi?> GetAsync(int id)
        {
            _logger.LogInformation("Api -> Exercise -> Get -> {id}", id);

            var exercise = await this.FindAsync(id);
            if (exercise is null)
                return null;

            return new ExerciseApi(exercise, include: true);
        }

        [HttpPost]
        public async Task<ExerciseApi?> PostAsync([FromBody] ExerciseApi exerciseApi)
        {
            _logger.LogInformation("Api -> Exercise -> Post");

            if (exerciseApi is null)
            {
                _logger.LogError("Argument {arg} cannot be null.", nameof(exerciseApi));
                return null;
            }

            var exercise = await _exerciseRepository.CreateAsync((Exercise)(IExercise)exerciseApi);
            return new ExerciseApi(exercise, include: true);
        }

        [HttpPut("{id}")]
        public async Task<ExerciseApi?> PutAsync(int id, [FromBody] ExerciseApi exerciseApi)
        {
            _logger.LogInformation("Api -> Exercise -> Put -> {id}", id);

            if (exerciseApi is null)
            {
                _logger.LogError("Argument {arg} cannot be null.", nameof(exerciseApi));
                return null;
            }

            var oldExercise = await this.FindAsync(id);
            if (oldExercise is null)
                return null;

            var exercise = await _exerciseRepository.UpdateAsync((Exercise)(IExercise)exerciseApi);
            return new ExerciseApi(exercise, include: true);
        }

        [HttpDelete("{id}")]
        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Api -> Exercise -> Delete -> {id}", id);

            var oldExercise = await this.FindAsync(id);
            if (oldExercise is null)
                return;

            await _exerciseRepository.DeleteAsync(id);
        }
    }
}
