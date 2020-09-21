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
    public class UserController : BaseController
    {
        private readonly ILogger<UserController> _logger;
        private readonly Repository<User> _userRepository;

        public UserController(PhoenixContext phoenixContext, ILogger<UserController> logger) : base(phoenixContext, logger)
        {
            this._logger = logger;
            this._userRepository = new Repository<User>(phoenixContext);
        }

        [HttpGet]
        public async Task<IEnumerable<IUser>> Get()
        {
            this._logger.LogInformation("Api -> User -> Get");

            IQueryable<User> users = this._userRepository.find();

            return await users.Select(user => new UserApi
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
                    FacebookId = user.AspNetUser.FacebookId,
                    RegisteredAt = user.AspNetUser.RegisteredAt
                }
            }).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<IUser> Get(int id)
        {
            this._logger.LogInformation($"Api -> User -> Get{id}");

            User user = await this._userRepository.find(id);

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
                    FacebookId = user.AspNetUser.FacebookId,
                    RegisteredAt = user.AspNetUser.RegisteredAt
                }
            };
        }



    }
}
