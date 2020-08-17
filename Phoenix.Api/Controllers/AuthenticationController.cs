using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Phoenix.Api.Models;
using Talagozis.AspNetCore.Services.TokenAuthentication;
using Talagozis.AspNetCore.Services.TokenAuthentication.Models;

namespace Phoenix.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : BaseController
    {
        private readonly ITokenAuthenticationService _tokenAuthenticationService;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(ITokenAuthenticationService tokenAuthenticationService, ILogger<AuthenticationController> logger)
        {
            this._tokenAuthenticationService = tokenAuthenticationService;
            this._logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("authenticate/basic")]
        public async Task<IActionResult> AuthenticateBasic([FromBody] TokenRequest tokenRequest)
        {
            this._logger.LogInformation("Api -> Authentication -> Authenticate -> Basic");

            if (tokenRequest == null)
                throw new ArgumentNullException(nameof(tokenRequest));

            if (!this.ModelState.IsValid)
                return this.BadRequest(this.ModelState);

            try
            {
                string token = await this._tokenAuthenticationService.authenticateAsync(tokenRequest);

                if (string.IsNullOrWhiteSpace(token))
                    return this.Unauthorized(new
                    {
                        code = 1,
                        message = "Bad user name or password."
                    });

                return this.Ok(new TokenResponse
                {
                    token = token
                });
            }
            catch (Exception ex)
            {
                this._logger.LogCritical(1, ex, nameof(this.AuthenticateBasic));
                throw;
            }
        }

        [AllowAnonymous]
        [HttpPost("authenticate/facebookid")]
        public async Task<IActionResult> AuthenticateFacebookId([FromBody] FacebookTokenRequest facebookTokenRequest)
        {
            throw new NotImplementedException();

            this._logger.LogInformation("Api -> Authentication -> Authenticate -> FacebookId");

            if (facebookTokenRequest == null)
                throw new ArgumentNullException(nameof(facebookTokenRequest));

            if (!this.ModelState.IsValid)
                return this.BadRequest(this.ModelState);

            try
            {
                //this._logger.LogInformation($"FacebookId: {facebookTokenRequest.facebookId} and signature: {facebookTokenRequest.signature}.");
                string token = await this._tokenAuthenticationService.authenticateAsync(new TokenRequest());

                if (string.IsNullOrWhiteSpace(token))
                    return this.Unauthorized(new
                    {
                        code = 1,
                        message = "Bad facebookId and signature."
                    });

                return this.Ok(new TokenResponse
                {
                    token = token
                });
            }
            catch (Exception ex)
            {
                this._logger.LogCritical(1, ex, nameof(this.AuthenticateFacebookId));
                throw;
            }
        }



    }
}