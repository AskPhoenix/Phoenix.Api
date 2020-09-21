using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Phoenix.DataHandle.Identity;
using Talagozis.AspNetCore.Models;

namespace Phoenix.Api.App_Plugins
{
    public class ApplicationSignInManager : SignInManager<ApplicationUser>, ISignInManager<ApplicationUser>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<ApplicationUser>> logger, IAuthenticationSchemeProvider schemes) 
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor,logger, schemes, new DefaultUserConfirmation<ApplicationUser>())
        {
        }

        public Task SignInAsync<TIdentity>(TIdentity user, bool isPersistent, string authenticationMethod = null) where TIdentity : IdentityUser<int>
        {
            ApplicationUser applicationUser = user as ApplicationUser ?? new ApplicationUser(user);

            return base.SignInAsync(applicationUser, isPersistent, authenticationMethod);
        }
    }
}