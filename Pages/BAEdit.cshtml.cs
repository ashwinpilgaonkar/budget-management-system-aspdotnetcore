using budget_management_system_aspdotnetcore.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using budget_management_system_aspdotnetcore.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Vml;
using Department = budget_management_system_aspdotnetcore.Entities.Department;
using System.Security.Claims;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class BAEditModel : PageModel
    {
        // ==============================================
        // Instance Variables -- HELPER
        // ==============================================
        #region HELPER
        private readonly CasdbtestContext _context;
        private readonly SpeedTypeService _speedTypeService;
        private readonly IAuthenticationService _authService;
        #endregion

        private readonly UserService _userService;

        public int userID { get; set; } = 0;

        public string userRole { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public int? SelectedBudgetAmendmentMainID { get; set; }

        public List<BudgetAmendmentMain> BudgetAmendmentsMain { get; set; }

        [BindProperty]
        public BudgetAmendmentMain NewBudgetAmendmentMain { get; set; }

        public DateTime BudgetAmendmentMainStartDate { get; set; }
        public DateTime BudgetAmendmentMainEndDate { get; set; }

        [BindProperty]
        public List<Department> Departments { get; set; }

        [BindProperty]
        public int BudgetAmendmentMainID { get; set; }

        [BindProperty]
        public DateTime ExtendDeadlineTo { get; set; }

        [BindProperty]
        public List<string> SelectedDepartments { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? selectedBA { get; set; }

        public List<string> FinancialYearOptions
        {
            get
            {
                var now = DateTime.Now;
                int currentFYStartYear = now.Month >= 10 ? now.Year : now.Year - 1;

                string currentFY = $"FY {currentFYStartYear}-{currentFYStartYear + 1}";
                string previousFY = $"FY {currentFYStartYear - 1}-{currentFYStartYear}";

                return new List<string> { currentFY, previousFY, "Custom" };
            }
        }

        public BAEditModel(CasdbtestContext context, SpeedTypeService speedTypeService, UserService userService, IAuthenticationService authService)
        {
            _context = context;
            _speedTypeService = speedTypeService;
            _userService = userService;
            _authService = authService;
        }

        public async Task LoadFormDataAsync()
        {
            userRole = _authService.GetUserRole(HttpContext);
            userID = _authService.GetAuthenticatedUserID(HttpContext);

            BudgetAmendmentsMain = await _context.BudgetAmendmentMain
            .OrderByDescending(ba => ba.CreatedAt)
            .ToListAsync();

            Departments = await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync();
        }


        public async Task<IActionResult> OnGetAsync(int amendmentPageNumber = 1, int amendmentResultsPerPage = 10)
        {
            if (!_authService.IsAuthenticated(HttpContext))
            {
                return RedirectToPage("/Login");
            }

            await LoadFormDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAddAmendmentMainAsync()
        {
            if (!ModelState.IsValid)
            {
                /*                return Page();*/
            }

            NewBudgetAmendmentMain.CreatedAt = DateTime.Now;
            NewBudgetAmendmentMain.CreatedBy = _authService.GetAuthenticatedUserID(HttpContext);
            NewBudgetAmendmentMain.UpdatedAt = DateTime.Now;
            NewBudgetAmendmentMain.UpdatedBy = _authService.GetAuthenticatedUserID(HttpContext);
            NewBudgetAmendmentMain.ExtendedDeadline = NewBudgetAmendmentMain.EndDate;

            _context.BudgetAmendmentMain.Add(NewBudgetAmendmentMain);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExtendDeadlineAsync(int BudgetAmendmentMainID, DateTime ExtendDeadlineTo)
        {
            var amendment = await _context.BudgetAmendmentMain.FindAsync(BudgetAmendmentMainID);

            if (amendment == null)
                return NotFound();

            if (ExtendDeadlineTo <= amendment.EndDate)
            {
                ModelState.AddModelError("", "Extended deadline must be after the current end date.");
                return Page();
            }

            bool applyToAll = SelectedDepartments.Contains("ALL");

            if (applyToAll)
            {
                amendment.ExtendedDeadline = ExtendDeadlineTo;

                // Remove all department overrides, because main date now applies to everyone
                var existingOverrides = _context.BADepartmentExtensions
                    .Where(e => e.BudgetAmendmentMainID == BudgetAmendmentMainID);

                _context.BADepartmentExtensions.RemoveRange(existingOverrides);
            }
            else
            {
                // For each chosen department, add/update override
                foreach (string deptIdStr in SelectedDepartments)
                {
                    int deptID = int.Parse(deptIdStr);

                    var existing = await _context.BADepartmentExtensions
                        .FirstOrDefaultAsync(e =>
                            e.BudgetAmendmentMainID == BudgetAmendmentMainID &&
                            e.DepartmentID == deptID);

                    if (existing == null)
                    {
                        _context.BADepartmentExtensions.Add(new BADepartmentExtension
                        {
                            BudgetAmendmentMainID = BudgetAmendmentMainID,
                            DepartmentID = deptID,
                            ExtendedDeadline = ExtendDeadlineTo
                        });
                    }
                    else
                    {
                        existing.ExtendedDeadline = ExtendDeadlineTo;
                    }
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task UpdateUserActivityLogAsync(string category, ActivityType action)
        {
            _context.UserActivityLogs.Add(new UserActivityLog
            {
                UserID = _authService.GetAuthenticatedUserID(HttpContext),
                Category = category,
                Description = action.ToString(),
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
    }
}