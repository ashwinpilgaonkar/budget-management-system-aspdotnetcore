using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace budget_management_system_aspdotnetcore.Entities
{
    public partial class Employee
    {
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime HireDate { get; set; }
        public string JobTitle { get; set; }
        public decimal Salary { get; set; }

        public int DepartmentID { get; set; }

        [ForeignKey("DepartmentID")]
        public virtual Department Department { get; set; }
    }
}
