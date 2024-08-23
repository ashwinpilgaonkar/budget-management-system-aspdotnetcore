namespace budget_management_system_aspdotnetcore.Entities
{
    public partial class Department
    {
        public int DepartmentID { get; set; }          // Primary Key
        public string DepartmentName { get; set; }     // Department Name
        public string Speedtype { get; set; }          // Unique Speedtype for the department
        public decimal Budget { get; set; }            // Department's budget
    }
}
