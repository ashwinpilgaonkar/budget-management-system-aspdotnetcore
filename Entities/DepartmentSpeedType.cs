using System.ComponentModel.DataAnnotations.Schema;

namespace budget_management_system_aspdotnetcore.Entities
{
    [Table("DepartmentSpeedType")]
    public partial class DepartmentSpeedType
    {
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        public int SpeedTypeId { get; set; }
        public SpeedType SpeedType { get; set; }
    }
}
