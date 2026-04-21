using budget_management_system_aspdotnetcore.Entities;

namespace budget_management_system_aspdotnetcore.ViewModels
{
    public class BAMainHeaderViewModel
    {
        public BudgetAmendmentMain Record { get; set; } = null!;
        public string Status { get; set; } = "None";
    }
}
