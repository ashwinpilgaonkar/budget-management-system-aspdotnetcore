namespace budget_management_system_aspdotnetcore.ViewModels
{
    public class FilterStateFullViewModel : FilterStateViewModel
    {
        public string? BudgetAmendmentSearchTerm { get; set; }
        public int? SelectedCreatedBy { get; set; }
        public DateTime? CreatedFromDate { get; set; }
        public DateTime? CreatedToDate { get; set; }
        public int? SelectedEditedBy { get; set; }
        public DateTime? EditedFromDate { get; set; }
        public DateTime? EditedToDate { get; set; }
        public int? SelectedUpdatedBy { get; set; }
        public DateTime? UpdatedFromDate { get; set; }
        public DateTime? UpdatedToDate { get; set; }
    }
}
