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
        public required string PhoneNumber { get; set; }
        public required DateTime HireDate { get; set; }
        public required string JobTitle { get; set; }
        public decimal Salary { get; set; }
        public int DepartmentID { get; set; }

        [ForeignKey("DepartmentID")]
        public virtual required Department Department { get; set; }

        public required string Password { get; set; }
        public required string Salt { get; set; }
        public UserRole Role { get; set; } = UserRole.admin;
        public UserStatus Status { get; set; } = UserStatus.active;
    }
    public enum UserRole
    {
        admin,
        user
    }

    public enum UserStatus
    {
        active,
        inactive
    }
}