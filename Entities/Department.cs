namespace budget_management_system_aspdotnetcore.Entities
{
    public partial class Department
    {
        public int DepartmentID { get; set; } 
        public string DepartmentName { get; set; }
        public string Speedtype { get; set; }
        public decimal Budget { get; set; }
    }
}
