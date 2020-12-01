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
        private readonly Repository<AspNetUsers> _aspNetUserRepository;

        public UserController(PhoenixContext phoenixContext, ILogger<UserController> logger) : base(phoenixContext, logger)
        {
            this._logger = logger;
            this._aspNetUserRepository = new Repository<AspNetUsers>(phoenixContext);
        }

        [HttpGet]
        public async Task<IEnumerable<IUser>> Get()
        {
            this._logger.LogInformation("Api -> AspNetUser -> Get");

            IQueryable<AspNetUsers> aspNetUsers = this._aspNetUserRepository.Find();

            return await aspNetUsers.Select(aspNetUser => new UserApi
            {
                id = aspNetUser.Id,
                LastName = aspNetUser.User.LastName,
                FirstName = aspNetUser.User.FirstName,
                FullName = aspNetUser.User.FullName,
                AspNetUser = new AspNetUserApi
                {
                    id = aspNetUser.Id,
                    UserName = aspNetUser.UserName,
                    Email = aspNetUser.Email,
                    PhoneNumber = aspNetUser.PhoneNumber,
                    RegisteredAt = aspNetUser.RegisteredAt
                }
            }).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<IUser> Get(int id)
        {
            this._logger.LogInformation($"Api -> AspNetUser -> Get{id}");

            AspNetUsers aspNetUser = await this._aspNetUserRepository.Find(id);

            return new UserApi
            {
                id = aspNetUser.Id,
                LastName = aspNetUser.User.LastName,
                FirstName = aspNetUser.User.FirstName,
                FullName = aspNetUser.User.FullName,
                AspNetUser = new AspNetUserApi
                {
                    id = aspNetUser.Id,
                    UserName = aspNetUser.UserName,
                    Email = aspNetUser.Email,
                    PhoneNumber = aspNetUser.PhoneNumber,
                    RegisteredAt = aspNetUser.RegisteredAt
                }
            };
        }



    }
}
