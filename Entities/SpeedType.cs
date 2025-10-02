using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace budget_management_system_aspdotnetcore.Entities
{
    public partial class SpeedType
    {
        public int SpeedTypeId { get; set; }

        public required string Code { get; set; }

        public decimal Budget { get; set; }
        public int FundCode { get; set; }
        public int ProgramCode { get; set; }
        public int ClassCode { get; set; }

        public required ICollection<DepartmentSpeedType> DepartmentSpeedTypes { get; set; }
    }
}
