﻿using System;
using System.Collections.Generic;
using System.Linq;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Relationships;

namespace Phoenix.Api.Models.Api
{
    public class AspNetUserApi : IAspNetUsers, IModelApi
    {
        public int id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string FacebookId { get; set; }
        public DateTime RegisteredAt { get; set; }

        public IUser User { get; set; }
        public IEnumerable<IAspNetUserRoles> Roles { get; set; }
    }
}
