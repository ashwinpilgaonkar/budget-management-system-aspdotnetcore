namespace budget_management_system_aspdotnetcore.Entities
{
    public partial class Department
    {
        public Department()
        {
            DepartmentSpeedTypes = new List<DepartmentSpeedType>(); // Initialize the collection
        }

        public int DepartmentID { get; set; }
        public required string DepartmentName { get; set; }

        // Navigation property to the join table
        public ICollection<DepartmentSpeedType> DepartmentSpeedTypes { get; set; }

        public virtual ICollection<User> AdminUsers { get; set; } = new List<User>();
    }
}
