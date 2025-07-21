using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class ActivityLogModel : PageModel
    {
        private readonly CasdbtestContext _context;
        private readonly IAuthenticationService _authService;

        public string userRole { get; set; } = "";
        public List<UserActivityLog> UserActivityLogs { get; set; }

        public ActivityLogModel(CasdbtestContext context, IAuthenticationService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<IActionResult> OnGetAsync(int amendmentPageNumber = 1, int amendmentResultsPerPage = 10)
        {
/*            BudgetAmendmentCurrentPage = amendmentPageNumber;
            BudgetAmendmentResultsPerPage = amendmentResultsPerPage;*/

            if (!_authService.IsAuthenticated(HttpContext))
            {
                return RedirectToPage("/Login");
            }

            await LoadFormDataAsync();
            return Page();
        }

        public async Task LoadFormDataAsync()
        {
            userRole = _authService.GetUserRole(HttpContext);

            UserActivityLogs = await _context.UserActivityLogs
                .Include(log => log.User)
                .OrderByDescending(log => log.Timestamp) // Optional: sort by newest
                .ToListAsync();
        }
    }
}
