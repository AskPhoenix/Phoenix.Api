using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Phoenix.Api.App_Plugins;
using Phoenix.Api.Models;
using Phoenix.Api.Models.Api;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class AccountController : BaseController
    {
        private readonly ILogger<AccountController> _logger;
        private readonly AspNetUserRepository _aspNetUserRepository;
        private readonly ApplicationUserManager _userManager;

        public AccountController(ApplicationUserManager userManager, PhoenixContext phoenixContext, ILogger<AccountController> logger) : base(phoenixContext, logger)
        {
            this._logger = logger;
            this._aspNetUserRepository = new AspNetUserRepository(phoenixContext);
            this._userManager = userManager;
            this._aspNetUserRepository.Include(a => a.Include(b => b.User));
        }

        [HttpGet("me")]
        public async Task<IUser> Me()
        {
            this._logger.LogInformation("Api -> Account -> Me");

            if(!this.userId.HasValue)
                throw new InvalidOperationException("AspNetUser is not authorized.");

            User user = (await this._aspNetUserRepository.Find().SingleAsync(a => a.Id == this.userId.Value)).User;

            return new UserApi
            {
                id = user.AspNetUserId,
                LastName = user.LastName,
                FirstName = user.FirstName,
                FullName = user.FullName,
                AspNetUser = new AspNetUserApi
                {
                    id = user.AspNetUser.Id,
                    UserName = user.AspNetUser.UserName,
                    Email = user.AspNetUser.Email,
                    PhoneNumber = user.AspNetUser.PhoneNumber,
                    RegisteredAt = user.AspNetUser.RegisteredAt,
                    TeacherCourses = user.AspNetUser.TeacherCourse.Select(teacherCourse => new TeacherCourseApi
                    {
                        Course = new CourseApi
                        {
                            id = teacherCourse.Course.Id,
                            Name = teacherCourse.Course.Name
                        }
                    }).ToList()
                }
            };
        }

        [HttpGet("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] AccountChangePassword accountChangePassword)
        {
            this._logger.LogInformation("Api -> Account -> ChangePassword");

            if (accountChangePassword == null)
                throw new ArgumentNullException(nameof(accountChangePassword));

            if (!this.userId.HasValue)
                throw new InvalidOperationException("AspNetUser is not authorized.");

            ApplicationUser applicationUser = await this._userManager.FindByIdAsync(this.userId.Value.ToString(CultureInfo.InvariantCulture));

            IdentityResult result = await this._userManager.ChangePasswordAsync(applicationUser, accountChangePassword.oldPassword, accountChangePassword.newPassword);

            if (!result.Succeeded)
            {
                this._logger.LogError(string.Join(", ", result.Errors.Select(a => $"{a.Code}: {a.Description}")));
                return this.BadRequest(new
                {
                    code = string.Join(", ", result.Errors.Select(a => a.Code)),
                    message = $"Could not change password: {string.Join(", ", result.Errors.Select(a => a.Description))}"
                });
            }

            return this.Ok();
        }



    }
}
