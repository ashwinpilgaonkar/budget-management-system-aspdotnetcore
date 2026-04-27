namespace budget_management_system_aspdotnetcore.ViewModels
{
    public class BAMainTabBarViewModel
    {
        public string? SelectedBAMainStatusTab { get; set; }
        public Dictionary<string, int> TabCounts { get; set; } = new();
        public string? SelectedFinancialYear { get; set; }
        public DateTime? CustomStartDate { get; set; }
        public DateTime? CustomEndDate { get; set; }
        public bool ShowOverdueOnly { get; set; }
        public bool BAMainCollapsed { get; set; }
        public int SelectedDepartmentID { get; set; }
        public int? SelectedBudgetAmendmentMainID { get; set; }
        public string? SelectedStatusTab { get; set; }
        public int? EditingBudgetAmendmentID { get; set; }
        public int SourceSpeedtype { get; set; }
        public int DestinationSpeedtype { get; set; }
    }
}
