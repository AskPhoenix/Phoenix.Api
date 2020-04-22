﻿using System;
using System.Collections.Generic;

namespace Phoenix.DataHandle.Models
{
    public partial class AspNetUsers
    {
        public AspNetUsers()
        {
            AspNetUserRoles = new HashSet<AspNetUserRoles>();
        }

        public int Id { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public int created_applicationType { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<AspNetUserRoles> AspNetUserRoles { get; set; }
    }
}
