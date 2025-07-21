using System.ComponentModel.DataAnnotations;

namespace budget_management_system_aspdotnetcore.Entities
{
    public partial class Role
    {
        public int RoleID { get; set; }

        [Required]
        [MaxLength(100)]
        public string RoleName { get; set; } = string.Empty;

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
