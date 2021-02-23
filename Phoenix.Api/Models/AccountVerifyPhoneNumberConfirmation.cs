using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;

namespace Phoenix.Api.Models
{
    public class AccountVerifyPhoneNumberConfirmation
    {
        [Required]
        [JsonProperty("phoneNumber")]
        public string phoneNumber { get; set; }

        [Required]
        [JsonProperty("pinCode")]
        public string pinCode { get; set; }

        [Required]
        [JsonProperty("requestPasswordResetToken")]
        public bool requestPasswordResetToken { get; set; }
    }
}
