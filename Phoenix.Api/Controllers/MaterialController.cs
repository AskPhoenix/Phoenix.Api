using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phoenix.DataHandle.Api.Models;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/[controller]")]
    public class MaterialController : ApplicationController
    {
        private readonly MaterialRepository _materialRepository;

        public MaterialController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<BookController> logger)
            : base(phoenixContext, userManager, logger)
        {
            _materialRepository = new(phoenixContext);
        }

        private async Task<Material?> FindAsync(int id)
        {
            if (!this.CheckUserAuth())
                return null;

            var material = await _materialRepository.FindPrimaryAsync(id);
            if (material is null)
            {
                _logger.LogError("No material found with id {id}", id);
                return null;
            }

            if (!material.Exam.Lecture.Course.Users.Any(u => u.AspNetUserId == this.AppUser!.Id))
            {
                _logger.LogError("User with id {UserId} " +
                    "is not authorized to access material with id {id}", id);
                return null;
            }

            return material;
        }

        [HttpGet("{id}")]
        public async Task<MaterialApi?> GetAsync(int id)
        {
            _logger.LogInformation("Api -> Material -> Get {id}", id);

            var material = await this.FindAsync(id);
            if (material is null)
                return null;

            return new MaterialApi(material);
        }

        [HttpPost]
        public async Task<MaterialApi?> PostAsync([FromBody] MaterialApi materialApi)
        {
            _logger.LogInformation("Api -> Material -> Post");

            if (materialApi is null)
            {
                _logger.LogError("Argument {arg} cannot be null.", nameof(materialApi));
                return null;
            }

            var material = await _materialRepository.CreateAsync(materialApi.ToMaterial());
            return new MaterialApi(material);
        }

        [HttpPut("{id}")]
        public async Task<MaterialApi?> PutAsync(int id, [FromBody] MaterialApi materialApi)
        {
            _logger.LogInformation("Api -> Material -> Put -> {id}", id);

            if (materialApi is null)
            {
                _logger.LogError("Argument {arg} cannot be null.", nameof(materialApi));
                return null;
            }

            var material = await this.FindAsync(id);
            if (material is null)
                return null;

            material = await _materialRepository.UpdateAsync(materialApi.ToMaterial(material));
            return new MaterialApi(material);
        }

        [HttpDelete("{id}")]
        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Api -> Material -> Get -> {id}", id);

            var material = await this.FindAsync(id);
            if (material is null)
                return;

            await _materialRepository.DeleteAsync(id);
        }
    }
}
