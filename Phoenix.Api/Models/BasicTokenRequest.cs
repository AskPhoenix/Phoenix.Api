using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Phoenix.Api.Models
{
    public class BasicTokenRequest
    {
        [JsonProperty("phone")]
        [Required]
        public string Phone { get; set; } = null!;

        [JsonProperty("password")]
        [Required]
        public string Password { get; set; } = null!;
    }
}
