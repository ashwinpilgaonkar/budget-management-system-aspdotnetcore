namespace budget_management_system_aspdotnetcore.Entities
{
    public partial class RolePermissionMapping
    {
        public int RoleID { get; set; }
        public Role Role { get; set; }

        public int PermissionID { get; set; }
        public Permission Permission { get; set; }
    }
}
