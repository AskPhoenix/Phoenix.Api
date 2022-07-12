using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Phoenix.Api.Models;
using Phoenix.DataHandle.Api.Models.Main;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;
using Phoenix.DataHandle.Sms;

namespace Phoenix.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ApplicationController
    {
        private readonly static TimeSpan pinCodeExpiration = new(0, 10, 0);

        private readonly OneTimeCodeRepository _otcRepository;
        private readonly ISmsService _smsService;

        public AccountController(
            ISmsService smsService,
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<AccountController> logger)
            : base(phoenixContext, userManager, logger)
        {
            _otcRepository = new(phoenixContext);
            _smsService = smsService;

            // TODO: Make sure these Includes are not needed
            //_userRepository.Include(u => u.Courses);
            //_userRepository.Include(u => u.OneTimeCodes);
        }

        [HttpGet("me")]
        public async Task<UserApi?> MeAsync(bool include = false)
        {
            _logger.LogInformation("Api -> Account -> Me");

            if (!this.CheckUserAuth())
                return null;

            User user = (await _userRepository.FindPrimaryAsync(AppUser!.Id))!;

            return new UserApi(user, include);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePasswordAsync(AccountChangePassword changePasswordModel)
        {
            _logger.LogInformation("Api -> Account -> ChangePassword");

            if (changePasswordModel is null)
                return BadRequest(nameof(changePasswordModel) + " argument cannot be null.");
            if (!this.CheckUserAuth())
                return Unauthorized();

            IdentityResult result = await _userManager.ChangePasswordAsync(
                this.AppUser!, changePasswordModel.OldPassword, changePasswordModel.NewPassword);

            if (!result.Succeeded)
            {
                _logger.LogError("{Errors}", 
                    string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}")));

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    error_code = string.Join(", ", result.Errors.Select(a => a.Code)),
                    error_message = $"Could not change password: " +
                        $"{string.Join(", ", result.Errors.Select(a => a.Description))}"
                });
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync(AccountResetPassword resetPasswordModel)
        {
            _logger.LogInformation("Api -> Account -> ResetPassword");

            if (resetPasswordModel is null)
                return BadRequest(nameof(resetPasswordModel) + " argument cannot be null.");

            var appUser = await _userManager.FindByIdAsync(resetPasswordModel.Id.ToString());
            if (appUser is null)
                return BadRequest($"Could not find a user with ID {resetPasswordModel.Id}");

            IdentityResult result = await _userManager.ResetPasswordAsync(appUser, resetPasswordModel.Token, resetPasswordModel.NewPassword);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Could not generate reset password, error: {string.Join(", ", result.Errors)}.");

            return Ok(new
            {
                id = appUser.Id,
                username = appUser.UserName,
                phone = appUser.PhoneNumber
            });
        }

        [AllowAnonymous]
        [HttpPost("verification/send-otc")]
        public async Task<IActionResult> SendVerificationOTCAsync(AccountSendVerificationOTC sendVerificationOTCModel)
        {
            _logger.LogInformation("Api -> Account -> Verification -> SendOTC");

            if (sendVerificationOTCModel is null)
                return BadRequest(nameof(sendVerificationOTCModel) + " argument cannot be null.");

            var appUser = await _userManager.FindByPhoneNumberAsync(sendVerificationOTCModel.PhoneNumber);
            if (appUser is null)
                return BadRequest($"Could not find a user with phone number {sendVerificationOTCModel.PhoneNumber}");

            OneTimeCode otc = new()
            {
                UserId = appUser.Id,
                Purpose = DataHandle.Main.Types.OneTimeCodePurpose.Verification,
                Token = new Random().Next(1111, 10000).ToString(),
                ExpiresAt = DateTime.UtcNow.Add(pinCodeExpiration)
            };
            otc = await _otcRepository.CreateAsync(otc);

            _smsService.Send(sendVerificationOTCModel.PhoneNumber, "Χρησιμοποιήστε το παρακάτω pin για την επαλήθευσή σας" +
                $" στο εργαλείο καθηγητών εντός 10 λεπτών: {otc.Token}");

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("verification/check-otc")]
        public async Task<IActionResult> CheckVerificationOTCAsync(AccountCheckVerificationOTC checkVerificationOTCModel)
        {
            _logger.LogInformation("Api -> Account -> -> Verification -> CheckOTC");

            if (checkVerificationOTCModel is null)
                return BadRequest(nameof(checkVerificationOTCModel) + " argument cannot be null.");

            var appUser = await _userManager.FindByPhoneNumberAsync(checkVerificationOTCModel.PhoneNumber);
            if (appUser is null)
                return BadRequest($"Could not find a user with phone number {checkVerificationOTCModel.PhoneNumber}.");

            User user = (await _userRepository.FindPrimaryAsync(appUser.Id))!;
            var userValidOTCs = user.OneTimeCodes
                .Where(c => c.ExpiresAt >= DateTime.UtcNow);

            if (!userValidOTCs.Any(c => c.Token == checkVerificationOTCModel.PinCode))
                return BadRequest($"Pin code is incorrect or has expired");

            appUser.PhoneNumberConfirmed = true;
            await _userManager.UpdateAsync(appUser);

            string? generatedResetPassword = null;
            if (checkVerificationOTCModel.RequestPasswordResetToken)
            {
                generatedResetPassword = await _userManager.GeneratePasswordResetTokenAsync(appUser);
                if (string.IsNullOrWhiteSpace(generatedResetPassword))
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        "Could not generate reset password");
            }

            return Ok(new
            {
                id = appUser.Id,
                username = appUser.UserName,
                phone = appUser.PhoneNumber,
                resetPasswordToken = generatedResetPassword
            });
        }
    }
}
