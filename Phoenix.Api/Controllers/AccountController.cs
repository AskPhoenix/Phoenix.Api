using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
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
    public class AccountController : BaseController
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserRepository _userRepository;

        public AccountController(PhoenixContext phoenixContext, ILogger<AccountController> logger) : base(phoenixContext, logger)
        {
            this._logger = logger;
            this._userRepository = new UserRepository(phoenixContext);
            this._userRepository.include(a => a.Include(b => b.AspNetUser));
        }

        [HttpGet("me")]
        public async Task<IUser> Me()
        {
            this._logger.LogInformation($"Api -> Account -> Me");

            if(!this.userId.HasValue)
                throw new Exception("User is not authorized.");

            User user = await this._userRepository.find().SingleAsync(a => a.AspNetUserId == this.userId.Value);

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
                },
                TeacherCourses = user.TeacherCourse.Select(teacherCourse => new TeacherCourseApi
                {
                    Course = new CourseApi
                    {
                        id = teacherCourse.Course.Id,
                        Name = teacherCourse.Course.Name
                    }
                }).ToList()
            };
        }



    }
}
