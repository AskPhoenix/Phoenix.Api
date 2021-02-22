using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;

namespace Phoenix.Api.Models
{
    public class AccountResetPassword
    {
        [Required]
        [JsonProperty("id")]
        public int id { get; set; }

        [Required]
        [JsonProperty("token")]
        public string token { get; set; }

        [Required]
        [JsonProperty("newPassword")]
        public string newPassword { get; set; }
    }
}
