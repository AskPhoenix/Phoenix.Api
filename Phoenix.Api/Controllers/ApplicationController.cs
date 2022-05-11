using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Phoenix.DataHandle.Identity;
using System.Security.Claims;

namespace Phoenix.Api.Controllers
{
    public abstract class ApplicationController : Controller
    {
        protected ApplicationUser? AppUser { get; private set; }

        protected readonly ILogger<ApplicationController> _logger;
        protected readonly ApplicationUserManager _userManager;

        protected ApplicationController(ILogger<ApplicationController> logger,
            ApplicationUserManager userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await base.OnActionExecutionAsync(context, next);

            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity is null)
            {
                _logger.LogTrace("No Identity is provided");
                return;
            }

            var userClaims = identity.Claims;
            if (!userClaims.Any(c => c.Type == ClaimTypes.NameIdentifier))
            {
                _logger.LogTrace("No claim for username found in the Identity");
                return;
            }

            this.AppUser = await _userManager.FindByNameAsync(userClaims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
            
            _logger.LogTrace("User with ID {Id} is authorized", this.AppUser.Id);
        }
    }
}
