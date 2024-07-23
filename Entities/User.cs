using System;
using System.Collections.Generic;
namespace budget_management_system_aspdotnetcore.Entities
{
    public partial class User
    {
        public int Id { get; set; }
        public int Email { get; set; }
        public string Password { get; set; }
    }
}


