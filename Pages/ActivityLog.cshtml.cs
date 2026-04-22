using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.Services;
using budget_management_system_aspdotnetcore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class ActivityLogModel : PageModel
    {
        private readonly CasdbtestContext _context;
        private readonly IAuthenticationService _authService;

        public string userRole { get; set; } = "";
        public List<UserActivityLog> UserActivityLogs { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int ResultsPerPage { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }

        public ActivityLogModel(CasdbtestContext context, IAuthenticationService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1, int resultsPerPage = 10)
        {
            if (!_authService.IsAuthenticated(HttpContext))
            {
                return RedirectToPage("/Login");
            }

            CurrentPage = pageNumber;
            ResultsPerPage = resultsPerPage;
            await LoadFormDataAsync();
            return Page();
        }

        public async Task LoadFormDataAsync()
        {
            userRole = _authService.GetUserRole(HttpContext);

            var query = _context.UserActivityLogs
                .Include(log => log.User)
                .OrderByDescending(log => log.Timestamp);

            TotalRecords = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalRecords / (double)ResultsPerPage);

            UserActivityLogs = await query
                .Skip((CurrentPage - 1) * ResultsPerPage)
                .Take(ResultsPerPage)
                .ToListAsync();
        }

        public PaginationViewModel GetActivityLogPagination()
        {
            return new PaginationViewModel
            {
                CurrentPage = CurrentPage,
                TotalPages = TotalPages,
                TotalRecords = TotalRecords,
                ResultsPerPage = ResultsPerPage,
                PageSizes = PaginationViewModel.DefaultPageSizes,
                AriaLabel = "Activity log page navigation",
                PrevUrl = $"?pageNumber={CurrentPage - 1}&resultsPerPage={ResultsPerPage}",
                NextUrl = $"?pageNumber={CurrentPage + 1}&resultsPerPage={ResultsPerPage}",
                SizeChangeUrlTemplate = $"?pageNumber=1&resultsPerPage=__SIZE__"
            };
        }
    }
}
