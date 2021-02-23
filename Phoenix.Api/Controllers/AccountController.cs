using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Phoenix.Api.App_Plugins;
using Phoenix.Api.Models;
using Phoenix.Api.Models.Api;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;
using Phoenix.DataHandle.Sms;

namespace Phoenix.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class AccountController : BaseController
    {
        private readonly static TimeSpan pinCodeExpiration = new TimeSpan(-1, 0, 0);

        private readonly ILogger<AccountController> _logger;
        private readonly AspNetUserRepository _aspNetUserRepository;
        private readonly ApplicationUserManager _userManager;
        private readonly ISmsService _smsService;

        public AccountController(ApplicationUserManager userManager, PhoenixContext phoenixContext, ISmsService smsService, ILogger<AccountController> logger) : base(phoenixContext, logger)
        {
            this._logger = logger;
            this._aspNetUserRepository = new AspNetUserRepository(phoenixContext);
            this._smsService = smsService;
            this._userManager = userManager;
            this._aspNetUserRepository.Include(a => a.Include(b => b.User));
        }

        [HttpGet("me")]
        public async Task<IUser> Me()
        {
            this._logger.LogInformation("Api -> Account -> Me");

            if(!this.userId.HasValue)
                throw new InvalidOperationException("AspNetUser is not authorized.");

            User user = (await this._aspNetUserRepository.Find().SingleAsync(a => a.Id == this.userId.Value)).User;

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
                    TeacherCourses = user.AspNetUser.TeacherCourse.Select(teacherCourse => new TeacherCourseApi
                    {
                        Course = new CourseApi
                        {
                            id = teacherCourse.Course.Id,
                            Name = teacherCourse.Course.Name
                        }
                    }).ToList()
                }
            };
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] AccountChangePassword accountChangePassword)
        {
            this._logger.LogInformation("Api -> Account -> ChangePassword");

            if (accountChangePassword == null)
                throw new ArgumentNullException(nameof(accountChangePassword));

            if (!this.userId.HasValue)
                throw new InvalidOperationException("AspNetUser is not authorized.");

            ApplicationUser applicationUser = await this._userManager.FindByIdAsync(this.userId.Value.ToString(CultureInfo.InvariantCulture));

            IdentityResult result = await this._userManager.ChangePasswordAsync(applicationUser, accountChangePassword.oldPassword, accountChangePassword.newPassword);

            if (!result.Succeeded)
            {
                this._logger.LogError(string.Join(", ", result.Errors.Select(a => $"{a.Code}: {a.Description}")));
                return this.BadRequest(new
                {
                    code = string.Join(", ", result.Errors.Select(a => a.Code)),
                    message = $"Could not change password: {string.Join(", ", result.Errors.Select(a => a.Description))}"
                });
            }

            return this.Ok();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] AccountResetPassword accountResetPassword)
        {
            this._logger.LogInformation("Api -> Account -> ResetPassword");

            if (accountResetPassword == null)
                throw new ArgumentNullException(nameof(accountResetPassword));

            if (accountResetPassword.id == default(uint) || string.IsNullOrWhiteSpace(accountResetPassword.token) || string.IsNullOrWhiteSpace(accountResetPassword.newPassword))
                throw new InvalidOperationException("There is an empty parameter.");


            AspNetUsers userT = this._aspNetUserRepository.Find().FirstOrDefault(a => a.Id == accountResetPassword.id);
            if (userT == null)
                throw new InvalidOperationException($"Could not find user by bid: {accountResetPassword.id}.");

            ApplicationUser applicationUser = await this._userManager.FindByIdAsync(userT.Id.ToString());
            if (applicationUser == null)
                throw new InvalidOperationException($"Could not find user by id.");

            IdentityResult result = await this._userManager.ResetPasswordAsync(applicationUser, accountResetPassword.token, accountResetPassword.newPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Could not generate reset password, error: {string.Join(", ", result.Errors)}.");

            return this.Ok(new 
            {
                id = applicationUser.Id,
                userName = applicationUser.UserName,
                phoneNumber = applicationUser.PhoneNumber
            });
        }

        [HttpPost("sendPhoneNumberConfirmation")]
        public async Task<IActionResult> SendPhoneNumberConfirmation([FromBody] AccountSendPhoneNumberConfirmation accountSendPhoneNumberConfirmationRpc)
        {
            this._logger.LogInformation("Api -> Account -> SendPhoneNumberConfirmation");

            if (accountSendPhoneNumberConfirmationRpc == null)
                throw new ArgumentNullException(nameof(accountSendPhoneNumberConfirmationRpc));

            if (string.IsNullOrWhiteSpace(accountSendPhoneNumberConfirmationRpc.phoneNumber))
                throw new InvalidOperationException($"There is an empty parameter.");

            try
            {
                ApplicationUser applicationUser = await this._userManager.FindByPhoneNumberAsync(accountSendPhoneNumberConfirmationRpc.phoneNumber);
                if (applicationUser == null)
                    return this.BadRequest(new { message = $"Could not find user by phone number." });

                Random rnd = new Random();
                int pinCode = rnd.Next(1, 10000);

                AspNetUsers aspNetUser = await this._aspNetUserRepository.Find(applicationUser.Id);
                aspNetUser.PhoneNumberVerificationCode = pinCode.ToString("0000");
                aspNetUser.PhoneNumberVerificationCode_at = DateTime.Now;
                aspNetUser = this._aspNetUserRepository.Update(aspNetUser);

                await this._smsService.SendAsync(accountSendPhoneNumberConfirmationRpc.phoneNumber, $"Χρησιμοποιήστε τον κωδικό επαλήθευσης {pinCode:0000} για τον έλεγχο ταυτότητας σας.");

                return this.Ok();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
                throw;
            }
        }

        [HttpPost("verifyPhoneNumberConfirmation")]
        public async Task<IActionResult> VerifyPhoneNumberConfirmation([FromBody] AccountVerifyPhoneNumberConfirmation accountVerifyPhoneNumberConfirmation)
        {
            this._logger.LogInformation("Api -> Account -> VerifyPhoneNumberConfirmation");

            if (accountVerifyPhoneNumberConfirmation == null)
                throw new ArgumentNullException(nameof(accountVerifyPhoneNumberConfirmation));

            if (string.IsNullOrEmpty(accountVerifyPhoneNumberConfirmation.phoneNumber) || string.IsNullOrEmpty(accountVerifyPhoneNumberConfirmation.pinCode))
                throw new InvalidOperationException("There is an empty parameter.");

            try
            {
                ApplicationUser applicationUser = await this._userManager.FindByPhoneNumberAsync(accountVerifyPhoneNumberConfirmation.phoneNumber);
                if (applicationUser == null)
                    throw new InvalidOperationException($"Could not find user by phone number: {accountVerifyPhoneNumberConfirmation.phoneNumber}.");

                AspNetUsers aspNetUser = await this._aspNetUserRepository.Find(applicationUser.Id);

                if (aspNetUser.PhoneNumberVerificationCode.ToUpperInvariant() != accountVerifyPhoneNumberConfirmation.pinCode.ToUpperInvariant())
                    throw new InvalidOperationException($"Pin code is incorrect.");

                if (aspNetUser.PhoneNumberVerificationCode_at < DateTime.Now.Add(pinCodeExpiration))
                    throw new InvalidOperationException($"Pin code is expired.");

                aspNetUser.PhoneNumberConfirmed = true;
                this._aspNetUserRepository.Update(aspNetUser);

                string generatedResetPassword = null;
                if (accountVerifyPhoneNumberConfirmation.requestPasswordResetToken)
                {
                    generatedResetPassword = await this._userManager.GeneratePasswordResetTokenAsync(applicationUser);
                    if (string.IsNullOrWhiteSpace(generatedResetPassword))
                        throw new InvalidOperationException($"Could not generate reset password.");
                }

                return this.Ok(new
                {
                    id = aspNetUser.Id,
                    userName = aspNetUser.UserName,
                    phoneNumber = aspNetUser.PhoneNumber,
                    resetPasswordToken = generatedResetPassword
                });
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
                throw;
            }
        }





    }
}
