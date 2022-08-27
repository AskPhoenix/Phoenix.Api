using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Phoenix.Api.Models
{
    public class AccountSendVerificationOTC
    {
        [Required]
        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; } = null!;
    }
}
