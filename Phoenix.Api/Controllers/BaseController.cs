using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    public abstract class BaseController : Talagozis.AspNetCore.Controllers.BaseController
    {
        protected int? userId { get; private set; }

        private readonly ILogger<BaseController> _logger;
        private readonly Repository<AspNetUsers> _aspNetAspNetUserRepository;

        protected BaseController(PhoenixContext phoenixContext, ILogger<BaseController> logger)
        {
            this._logger = logger;
            this._aspNetAspNetUserRepository = new Repository<AspNetUsers>(phoenixContext);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            this.userId = this._aspNetAspNetUserRepository.Find().SingleOrDefault(a => a.UserName == this.User.Identity.Name)?.Id;

            this._logger.LogTrace(this.userId.HasValue ? $"The userId is set to {this.userId.Value}" : "No userId is set");
        }
    }
}