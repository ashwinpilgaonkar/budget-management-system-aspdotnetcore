using System.ComponentModel.DataAnnotations;

namespace budget_management_system_aspdotnetcore.Entities
{
    public partial class Permission
    {
        public int PermissionID { get; set; }

        [Required]
        [MaxLength(100)]
        public string PermissionName { get; set; }

        public ICollection<RolePermissionMapping> RolePermissionMappings { get; set; } = new List<RolePermissionMapping>();
    }
}
