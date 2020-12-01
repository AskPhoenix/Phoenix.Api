using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;
using Talagozis.AspNetCore.Models;

namespace Phoenix.Api.App_Plugins
{
    public class ApplicationUserManager : UserManager<ApplicationUser>, IUserManager<ApplicationUser>
    {
        protected internal new ApplicationStore Store => base.Store as ApplicationStore ?? throw new NotSupportedException($"{nameof(base.Store)} is not a {nameof(ApplicationStore)} type.");

        public ApplicationUserManager(IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators, IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<ApplicationUserManager> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        async Task<IdentityUser<int>> IUserManager<ApplicationUser>.FindByNameAsync(string userName)
        {
            return await this.FindByNameAsync(userName);
        }

        Task<IdentityResult> IUserManager<ApplicationUser>.CreateAsync<TIdentity>(TIdentity user, string password)
        {
            ApplicationUser applicationUser = user as ApplicationUser ?? new ApplicationUser(user);

            applicationUser.CreatedAt = DateTime.Now;
            if (applicationUser.User == null)
                applicationUser.User = new User();

            return this.CreateAsync(applicationUser, password);
        }

        Task<string> IUserManager<ApplicationUser>.GenerateEmailConfirmationTokenAsync<TIdentity>(TIdentity user)
        {
            ApplicationUser applicationUser = user as ApplicationUser ?? new ApplicationUser(user);

            return base.GenerateEmailConfirmationTokenAsync(applicationUser);
        }

        public override Task<IdentityResult> CreateAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.CreatedAt = DateTime.Now;
            if (user.User == null)
                user.User = new User();

            return base.CreateAsync(user);
        }

        public Task<ApplicationUser> FindByPhoneNumberAsync(string phoneNumber)
        {
            return this.Store.FindByPhoneNumberAsync(phoneNumber);
        }

        //public Task<ApplicationUser> FindByFacebookIdAsync(string facebookId)
        //{
        //    return this.Store.FindByFacebookIdAsync(facebookId);
        //}
    }
}