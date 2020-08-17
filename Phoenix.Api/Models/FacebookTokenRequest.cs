using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;

namespace Phoenix.Api.Models
{
    public class FacebookTokenRequest
    {
        [Required]
        [JsonProperty("facebookId")]
        public string facebookId { get; set; }

        [Required]
        [JsonProperty("signature")]
        public string signature { get; set; }
    }
}
