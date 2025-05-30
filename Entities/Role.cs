using System.ComponentModel.DataAnnotations;

namespace budget_management_system_aspdotnetcore.Entities
{
    public partial class Role
    {
        public int RoleID { get; set; }

        [Required]
        [MaxLength(100)]
        public string RoleName { get; set; }

        public ICollection<RolePermissionMapping> RolePermissionMappings { get; set; } = new List<RolePermissionMapping>();

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
