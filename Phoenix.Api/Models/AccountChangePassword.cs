using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;

namespace Phoenix.Api.Models
{
    public class AccountChangePassword
    {
        [Required]
        [JsonProperty("oldPassword")]
        public string oldPassword { get; set; }

        [Required]
        [JsonProperty("newPassword")]
        public string newPassword { get; set; }
    }
}
