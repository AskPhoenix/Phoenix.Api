using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phoenix.DataHandle.Api.Models.Main;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ExerciseController : Controller
    {
        private readonly ILogger<ExerciseController> _logger;
        private readonly ExerciseRepository _exerciseRepository;

        public ExerciseController(ILogger<ExerciseController> logger,
            PhoenixContext phoenixContext)
        {
            _logger = logger;
            _exerciseRepository = new ExerciseRepository(phoenixContext);
        }

        [HttpGet("{id}")]
        public async Task<ExerciseApi?> GetAsync(int id)
        {
            _logger.LogInformation("Api -> Exercise -> Get -> {id}", id);

            var exercise = await _exerciseRepository.FindPrimaryAsync(id);
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

            var exercise = await _exerciseRepository.UpdateAsync((Exercise)(IExercise)exerciseApi);
            return new ExerciseApi(exercise, include: true);
        }

        [HttpDelete("{id}")]
        public async void Delete(int id)
        {
            _logger.LogInformation("Api -> Exercise -> Delete -> {id}", id);

            await _exerciseRepository.DeleteAsync(id);
        }
    }
}
