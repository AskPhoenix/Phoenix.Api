using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;
using Talagozis.AspNetCore.Services.TokenAuthentication;
using Talagozis.AspNetCore.Services.TokenAuthentication.Models;

namespace Phoenix.Api.App_Plugins
{
    public class UserManagementService : IUserManagementService
    {
        private readonly ApplicationUserManager _userManager;
        private readonly AspNetUserRepository _aspNetUserRepository;

        public UserManagementService(ApplicationUserManager userManager, PhoenixContext phoenixContext)
        {
            this._userManager = userManager;
            this._aspNetUserRepository = new AspNetUserRepository(phoenixContext);
        }

        public async Task<IAuthenticatedUser> authenticateUserBasicAsync(string username, string password, CancellationToken cancellationToken)
        {
            ApplicationUser applicationUser = await this._userManager.FindByNameAsync(username);

            if (applicationUser == null)
                return null;

            // TODO: To be refactored
            if (!applicationUser.PhoneNumberConfirmed)
                return null;

            if (!await this._userManager.CheckPasswordAsync(applicationUser, password))
                return null;

            return new AuthenticatedUser
            {
                uuid = applicationUser.Id.ToString(),
                username = applicationUser.UserName,
                email = applicationUser.EmailConfirmed ? applicationUser.Email : string.Empty,
                phoneNumber = applicationUser.PhoneNumberConfirmed ? applicationUser.PhoneNumber : string.Empty,
                roles = (await this._userManager.GetRolesAsync(applicationUser)).ToArray(),
            };
        }

        public async Task<IAuthenticatedUser> authenticateUserFacebookIdAsync(string facebookId, string signature, CancellationToken cancellationToken)
        {
            var user = this._aspNetUserRepository.find().SingleOrDefault(a => a.FacebookId == facebookId);
            if (user == null)
                return null;

            ApplicationUser applicationUser = await this._userManager.FindByIdAsync(user.Id.ToString());

            if (applicationUser == null)
                return null;

            // TODO: To be refactored
            if (!applicationUser.PhoneNumberConfirmed)
                return null;

            if (!(await this._aspNetUserRepository.find(applicationUser.Id)).verifyHashSignature(signature))
                return null;

            return new AuthenticatedUser
            {
                uuid = applicationUser.Id.ToString(),
                username = applicationUser.UserName,
                email = applicationUser.EmailConfirmed ? applicationUser.Email : string.Empty,
                phoneNumber = applicationUser.PhoneNumberConfirmed ? applicationUser.PhoneNumber : string.Empty,
                roles = (await this._userManager.GetRolesAsync(applicationUser)).ToArray(),
            };
        }

    }
}
