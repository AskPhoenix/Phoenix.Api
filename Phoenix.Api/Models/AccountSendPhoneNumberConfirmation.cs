using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;

namespace Phoenix.Api.Models
{
    public class AccountSendPhoneNumberConfirmation
    {
        [Required]
        [JsonProperty("phoneNumber")]
        public string phoneNumber { get; set; }
    }
}
