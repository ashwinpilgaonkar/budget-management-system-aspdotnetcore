using System;
using System.Collections.Generic;
namespace budget_management_system_aspdotnetcore.Entities
{
    public partial class User
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public string Salt { get; set; }
        public string Role { get; set; }

        public UserStatus Status { get; set; } = UserStatus.active;
    }

    public enum UserStatus
    {
        active,
        inactive
    }
}