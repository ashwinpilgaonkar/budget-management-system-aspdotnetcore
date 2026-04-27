namespace budget_management_system_aspdotnetcore.ViewModels
{
    public class FilterStateViewModel
    {
        public int SelectedDepartmentID { get; set; }
        public string SelectedStatusTab { get; set; } = "";
        public string? SelectedBAMainStatusTab { get; set; }
        public int? SelectedBudgetAmendmentMainID { get; set; }
        public string SelectedFinancialYear { get; set; } = "";
        public DateTime? CustomStartDate { get; set; }
        public DateTime? CustomEndDate { get; set; }
    }
}
