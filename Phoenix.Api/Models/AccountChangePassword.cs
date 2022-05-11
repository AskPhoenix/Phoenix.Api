using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Phoenix.Api.Models
{
    public class AccountChangePassword
    {
        [Required]
        [JsonProperty("old_password")]
        public string OldPassword { get; set; } = null!;

        [Required]
        [JsonProperty("new_password")]
        public string NewPassword { get; set; } = null!;
    }
}
