namespace budget_management_system_aspdotnetcore.ViewModels
{
    public class PaginationViewModel
    {
        public static readonly List<int> DefaultPageSizes = [10, 20, 30];

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public int ResultsPerPage { get; set; }
        public List<int> PageSizes { get; set; }
        public string PrevUrl { get; set; }
        public string NextUrl { get; set; }
        public string SizeChangeUrlTemplate { get; set; }
        public string AriaLabel { get; set; }
    }
}
