using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace budget_management_system_aspdotnetcore.Entities
{
    public partial class User
    {
        public int UserId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }

        public virtual ICollection<Department> DepartmentsResponsibleFor { get; set; } = new List<Department>();

        public required string Password { get; set; }

        // 🔁 Replace enum with Role entity
        public int RoleID { get; set; }

        [ForeignKey("RoleID")]
        public virtual required Role Role { get; set; }

        public UserStatus Status { get; set; } = UserStatus.active;
    }

    public enum UserStatus
    {
        active,
        inactive
    }
}