using budget_management_system_aspdotnetcore.Entities;

namespace budget_management_system_aspdotnetcore.Entities
{
    public class BADepartmentExtension
    {
        public int BADepartmentExtensionID { get; set; }

        public int BudgetAmendmentMainID { get; set; }
        public BudgetAmendmentMain BudgetAmendmentMain { get; set; }

        public int DepartmentID { get; set; }
        public Department Department { get; set; }

        public DateTime ExtendedDeadline { get; set; }
    }
}
