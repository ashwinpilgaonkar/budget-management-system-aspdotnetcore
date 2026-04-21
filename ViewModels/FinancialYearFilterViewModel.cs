namespace budget_management_system_aspdotnetcore.ViewModels
{
    public class FinancialYearFilterViewModel
    {
        public List<string> FinancialYearOptions { get; set; } = new();
        public string? SelectedFinancialYear { get; set; }
        public DateTime? CustomStartDate { get; set; }
        public DateTime? CustomEndDate { get; set; }
        public Dictionary<string, string> HiddenFields { get; set; } = new();
    }
}
