using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.Services;
using budget_management_system_aspdotnetcore.Helpers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly CasdbtestContext _context;
        private readonly IAuthenticationService _authService;

        public string userRole { get; set; } = "";
        private int userID { get; set; }

        public int OverviewTotalCount { get; set; }
        public int OverviewApprovedCount { get; set; }
        public int OverviewPendingCount { get; set; }
        public int OverviewRejectedCount { get; set; }
        public int OverviewTotalActionsThisMonth { get; set; }
        public string OverviewMostActiveUser { get; set; } = "";
        public DateTime OverviewLastActivityTime { get; set; }

        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
        public string? SelectedFinancialYear { get; set; }

        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
        public DateTime? CustomStartDate { get; set; }

        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
        public DateTime? CustomEndDate { get; set; }

        public List<string> FinancialYearOptions => FinancialYearHelper.GetOptions();

        public DashboardModel(CasdbtestContext context, IAuthenticationService authService)
        {
            _context = context;
            _authService = authService;
        }

        public void OnGet()
        {
            userRole = _authService.GetUserRole(HttpContext);
            userID = _authService.GetAuthenticatedUserID(HttpContext);

            SetDefaultFinancialYearRange();
            SetOverviewCardData();
        }

        private void SetDefaultFinancialYearRange()
        {
            var today = DateTime.Today;
            int startYear = today.Month >= 7 ? today.Year : today.Year - 1;

            if (!CustomStartDate.HasValue)
                CustomStartDate = new DateTime(startYear, 7, 1);

            if (!CustomEndDate.HasValue)
                CustomEndDate = new DateTime(startYear + 1, 6, 30);
        }

        private void SetOverviewCardData()
        {
            DateTime startDate, endDate;

            if (string.IsNullOrEmpty(SelectedFinancialYear) && FinancialYearOptions.Any())
                SelectedFinancialYear = FinancialYearOptions.First();

            (startDate, endDate) = FinancialYearHelper.GetDateRange(SelectedFinancialYear, CustomStartDate, CustomEndDate);

            var amendmentQuery = _context.BudgetAmendments
                .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate);

            if (userRole == "5")
                amendmentQuery = amendmentQuery.Where(a => a.CreatedBy == userID);

            var amendments = amendmentQuery.ToList();

            OverviewTotalCount = amendments.Count;
            OverviewApprovedCount = amendments.Count(a => a.Status == AmendmentStatus.Approved);
            OverviewPendingCount = amendments.Count(a => a.Status == AmendmentStatus.Pending);
            OverviewRejectedCount = amendments.Count(a => a.Status == AmendmentStatus.Rejected);

            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var recentLogs = _context.UserActivityLogs
                .Include(log => log.User)
                .Where(log => log.Timestamp >= startOfMonth)
                .ToList();

            OverviewTotalActionsThisMonth = recentLogs.Count;
            OverviewMostActiveUser = recentLogs
                .GroupBy(log => log.User.Email)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";
            OverviewLastActivityTime = recentLogs
                .OrderByDescending(log => log.Timestamp)
                .Select(log => log.Timestamp)
                .FirstOrDefault();
        }
    }
}
