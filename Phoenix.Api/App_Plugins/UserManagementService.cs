using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(ApplicationUserManager userManager, PhoenixContext phoenixContext, ILogger<UserManagementService> logger)
        {
            this._userManager = userManager;
            this._aspNetUserRepository = new AspNetUserRepository(phoenixContext);
            this._logger = logger;
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
            var user = await this._aspNetUserRepository.find().SingleOrDefaultAsync(a => a.FacebookId == facebookId, cancellationToken: cancellationToken);
            if (user == null)
            {
                this._logger.LogDebug($"{nameof(facebookId)}: {facebookId}, {nameof(signature)}: {signature}");
                this._logger.LogDebug($"No {nameof(user)} has found");
                return null;
            }

            ApplicationUser applicationUser = await this._userManager.FindByIdAsync(user.Id.ToString());

            if (applicationUser == null)
            {
                this._logger.LogDebug($"{nameof(facebookId)}: {facebookId}, {nameof(signature)}: {signature}");
                this._logger.LogDebug($"No {nameof(applicationUser)} has found");

                return null;
            }

            // TODO: To be refactored
            //if (!applicationUser.PhoneNumberConfirmed)
            //    return null;

            if (!user.verifyHashSignature(signature))
            {
                this._logger.LogDebug($"{nameof(facebookId)}: {facebookId}, {nameof(signature)}: {signature}");
                this._logger.LogDebug($"The verifyHashSignature failed. Generated signature: {user.getHashSignature()}");
                return null;
            }

            return new AuthenticatedUser
            {
                uuid = applicationUser.Id.ToString(),
                username = applicationUser.UserName,
                email = applicationUser.EmailConfirmed ? (applicationUser.Email ?? string.Empty) : string.Empty,
                phoneNumber = applicationUser.PhoneNumberConfirmed ? (applicationUser.PhoneNumber ?? string.Empty) : string.Empty,
                roles = (await this._userManager.GetRolesAsync(applicationUser)).ToArray(),
            };
        }

    }
}
