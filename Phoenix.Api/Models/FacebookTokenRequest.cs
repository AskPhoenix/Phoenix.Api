using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Phoenix.Api.Models
{
    public class FacebookTokenRequest
    {
        [Required]
        [JsonProperty("FacebookId")]
        public string FacebookId { get; set; } = null!;

        [Required]
        [JsonProperty("signature")]
        public string Signature { get; set; } = null!;
    }
}
