using System;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly AspNetUserRepository _aspNetAspNetUserRepository;
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(ApplicationUserManager userManager, PhoenixContext phoenixContext, ILogger<UserManagementService> logger)
        {
            this._userManager = userManager;
            this._aspNetAspNetUserRepository = new AspNetUserRepository(phoenixContext);
            this._logger = logger;
        }

        public async Task<IAuthenticatedUser> authenticateUserBasicAsync(string username, string password, CancellationToken cancellationToken)
        {
            ApplicationUser applicationUser = await this._userManager.FindByPhoneNumberAsync(username);

            if (applicationUser == null)
            {
                this._logger.LogDebug($"No {nameof(applicationUser)} has found");
                this._logger.LogDebug($"{nameof(username)}: {username}");
                return null;
            }

            // TODO: To be refactored
            if (!applicationUser.PhoneNumberConfirmed)
            {
                this._logger.LogDebug($"The phone number {applicationUser.PhoneNumber} is not confirmed");
                this._logger.LogDebug($"{nameof(username)}: {username}");
                return null;
            }

            if (!await this._userManager.CheckPasswordAsync(applicationUser, password))
            {
                this._logger.LogDebug($"The password is not correct");
                this._logger.LogDebug($"{nameof(username)}: {username}");
                return null;
            }

            return new AuthenticatedUser
            {
                uuid = applicationUser.Id.ToString(CultureInfo.InvariantCulture),
                username = applicationUser.UserName,
                email = applicationUser.EmailConfirmed ? (applicationUser.Email ?? string.Empty) : string.Empty,
                phoneNumber = applicationUser.PhoneNumberConfirmed ? (applicationUser.PhoneNumber ?? string.Empty) : string.Empty,
                roles = (await this._userManager.GetRolesAsync(applicationUser)).ToArray(),
            };
        }

        public async Task<IAuthenticatedUser> authenticateUserFacebookIdAsync(string facebookId, string signature, CancellationToken cancellationToken)
        {
            var user = await this._aspNetAspNetUserRepository.Find().SingleOrDefaultAsync(a => a.AspNetUserLogins.Any(b => b.LoginProvider == "Facebook" && b.ProviderKey == facebookId), cancellationToken: cancellationToken);
            if (user == null)
            {
                this._logger.LogDebug($"{nameof(facebookId)}: {facebookId}, {nameof(signature)}: {signature}");
                this._logger.LogDebug($"No {nameof(user)} has found");
                return null;
            }

            ApplicationUser applicationUser = await this._userManager.FindByIdAsync(user.Id.ToString(CultureInfo.InvariantCulture));

            if (applicationUser == null)
            {
                this._logger.LogDebug($"{nameof(facebookId)}: {facebookId}, {nameof(signature)}: {signature}");
                this._logger.LogDebug($"No {nameof(applicationUser)} has found");

                return null;
            }

            // TODO: To be refactored
            //if (!applicationUser.PhoneNumberConfirmed)
            //    return null;

            if (!user.VerifyHashSignature(signature))
            {
                this._logger.LogDebug($"{nameof(facebookId)}: {facebookId}, {nameof(signature)}: {signature}");
                this._logger.LogDebug($"The verifyHashSignature failed. Generated signature: {user.GetHashSignature()}");
                return null;
            }

            return new AuthenticatedUser
            {
                uuid = applicationUser.Id.ToString(CultureInfo.InvariantCulture),
                username = applicationUser.UserName,
                email = applicationUser.EmailConfirmed ? (applicationUser.Email ?? string.Empty) : string.Empty,
                phoneNumber = applicationUser.PhoneNumberConfirmed ? (applicationUser.PhoneNumber ?? string.Empty) : string.Empty,
                roles = (await this._userManager.GetRolesAsync(applicationUser)).ToArray(),
            };
        }

    }
}
