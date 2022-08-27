using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Phoenix.Api.Models
{
    public class AccountCheckVerificationOTC
    {
        [Required]
        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        [JsonProperty("pin_code")]
        public string PinCode { get; set; } = null!;

        [Required]
        [JsonProperty("request_password_reset_token")]
        public bool RequestPasswordResetToken { get; set; }
    }
}
