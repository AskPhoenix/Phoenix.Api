using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phoenix.DataHandle.Api.Models.Main;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Main.Types;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class LectureController : ApplicationController
    {
        private readonly LectureRepository _lectureRepository;

        public LectureController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<LectureController> logger)
            : base(phoenixContext, userManager, logger)
        {
            _lectureRepository = new(phoenixContext);
        }

        private async Task<Lecture?> FindAsync(int id)
        {
            if (!this.CheckUserAuth())
                return null;

            var lecture = await _lectureRepository.FindPrimaryAsync(id);
            if (lecture is null)
            {
                _logger.LogError("No lecture found with id {id}", id);
                return null;
            }

            if (!lecture.Course.Users.Any(u => u.AspNetUserId == this.AppUser!.Id))
            {
                _logger.LogError("User with id {UserId} " +
                    "is not authorized to access lecture with id {id}", id);
                return null;
            }

            return lecture;
        }

        [HttpGet("{id}")]
        public async Task<LectureApi?> GetAsync(int id)
        {
            _logger.LogInformation("Api -> Lecture -> Get {id}", id);

            var lecture = await _lectureRepository.FindPrimaryAsync(id);
            if (lecture is null)
                return null;

            return new LectureApi(lecture, include: true);
        }

        [HttpGet("{id}/Exercises")]
        public async Task<IEnumerable<ExerciseApi>?> GetExercisesAsync(int id)
        {
            _logger.LogInformation("Api -> Lecture -> Get -> {id} -> Exercises", id);

            var lecture = await this.FindAsync(id);
            if (lecture is null)
                return null;

            return lecture.Exercises.Select(e => new ExerciseApi(e, include: true));
        }

        [HttpGet("{id}/Exams")]
        public async Task<IEnumerable<ExamApi>?> GetExamsAsync(int id)
        {
            _logger.LogInformation("Api -> Lecture -> Get -> {id} -> Exams", id);

            var lecture = await this.FindAsync(id);
            if (lecture is null)
                return null;

            return lecture.Exams.Select(e => new ExamApi(e, include: true));
        }

        [HttpGet("{id}/Students")]
        public async Task<IEnumerable<UserApi>?> GetStudentsAsync(int id)
        {
            _logger.LogInformation("Api -> Lecture -> Get -> {id} -> Students", id);

            var lecture = await this.FindAsync(id);
            if (lecture is null)
                return null;

            // TODO: Try to improve
            var users = lecture.Course.Users;
            foreach (var user in users)
            {
                var appuser = await _userManager.FindByIdAsync(user.AspNetUserId.ToString());
                if (!await _userManager.IsInRoleAsync(appuser, RoleRank.Student.ToNormalizedString()))
                    users.Remove(user);
            }

            return users.Select(u => new UserApi(u, include: true));
        }

        [HttpPost]
        public async Task<LectureApi?> PostAsync([FromBody] LectureApi lectureApi)
        {
            _logger.LogInformation("Api -> Lecture -> Post");

            if (lectureApi is null)
            {
                _logger.LogError("Argument {arg} cannot be null.", nameof(lectureApi));
                return null;
            }

            var lecture = await _lectureRepository.CreateAsync((Lecture)(ILecture)lectureApi);
            return new LectureApi(lecture, include: true);
        }
    }
}
