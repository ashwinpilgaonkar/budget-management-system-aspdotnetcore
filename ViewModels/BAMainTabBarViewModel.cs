namespace budget_management_system_aspdotnetcore.ViewModels
{
    public class BAMainTabBarViewModel
    {
        public string? SelectedBAMainStatusTab { get; set; }
        public Dictionary<string, int> TabCounts { get; set; } = new();
        public string? SelectedFinancialYear { get; set; }
        public DateTime? CustomStartDate { get; set; }
        public DateTime? CustomEndDate { get; set; }
    }
}
