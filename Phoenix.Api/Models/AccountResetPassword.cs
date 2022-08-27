using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Phoenix.Api.Models
{
    public class AccountResetPassword
    {
        [Required]
        [JsonProperty("id")]
        public int Id { get; set; }

        [Required]
        [JsonProperty("token")]
        public string Token { get; set; } = null!;

        [Required]
        [JsonProperty("new_password")]
        public string NewPassword { get; set; } = null!;
    }
}
