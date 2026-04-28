using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using budget_management_system_aspdotnetcore.Services;
using budget_management_system_aspdotnetcore.Helpers;
using Department = budget_management_system_aspdotnetcore.Entities.Department;

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

        public List<BudgetAmendmentMain> BudgetAmendmentsMain { get; set; }
        public List<BudgetAmendmentMain> BudgetAmendmentsMainAll { get; set; }
        public List<BudgetAmendment> BudgetAmendmentsAll { get; set; }

        [BindProperty]
        public BudgetAmendmentMain NewBudgetAmendmentMain { get; set; }

        public List<Department> Departments { get; set; }

        [BindProperty]
        public int BudgetAmendmentMainID { get; set; }

        [BindProperty]
        public DateTime ExtendDeadlineTo { get; set; }

        [BindProperty]
        public List<string> SelectedDepartments { get; set; }

        public Dictionary<int, int> DeptExtensionCounts { get; set; }

        public List<int> PageSizes { get; set; } = PaginationViewModel.DefaultPageSizes;

        public int BAMainCurrentPage { get; set; } = 1;
        public int BAMainResultsPerPage { get; set; } = 10;
        public int BAMainTotalPages { get; set; }
        public int TotalBAMains { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedBAMainStatusTab { get; set; } = "Submitted";

        [BindProperty(SupportsGet = true)]
        public bool ShowOverdueOnly { get; set; } = false;

        [BindProperty(SupportsGet = true)]
        public bool BAMainCollapsed { get; set; } = false;

        [BindProperty(SupportsGet = true)]
        public string SelectedFinancialYear { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public DateTime? CustomStartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? CustomEndDate { get; set; }

        public List<string> FinancialYearOptions => FinancialYearHelper.GetOptions();

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

            if (string.IsNullOrEmpty(SelectedFinancialYear) && FinancialYearOptions.Any())
                SelectedFinancialYear = FinancialYearOptions.First();

            var today = DateTime.Today;
            int fyStartYear = today.Month >= 7 ? today.Year : today.Year - 1;
            if (!CustomStartDate.HasValue) CustomStartDate = new DateTime(fyStartYear, 7, 1);
            if (!CustomEndDate.HasValue) CustomEndDate = new DateTime(fyStartYear + 1, 6, 30);

            var (startDate, endDate) = FinancialYearHelper.GetDateRange(SelectedFinancialYear, CustomStartDate, CustomEndDate);

            BudgetAmendmentsAll = await _context.BudgetAmendments.ToListAsync();

            BudgetAmendmentsMain = await _context.BudgetAmendmentMain
                .Where(ba => ba.ExtendedDeadline >= startDate && ba.ExtendedDeadline <= endDate)
                .OrderByDescending(ba => ba.CreatedAt)
                .ToListAsync();

            BudgetAmendmentsMainAll = BudgetAmendmentsMain.ToList();

            if (!string.IsNullOrEmpty(SelectedBAMainStatusTab))
            {
                BudgetAmendmentsMain = BudgetAmendmentsMain.Where(bam =>
                {
                    var related = BudgetAmendmentsAll.Where(ba => ba.BudgetAmendmentMainID == bam.BudgetAmendmentMainID).ToList();
                    return SelectedBAMainStatusTab switch
                    {
                        "Submitted" => related.Any(ba => ba.Status == AmendmentStatus.Submitted),
                        "Approved"  => related.Any(ba => ba.Status == AmendmentStatus.Approved)
                            && !related.Any(ba => ba.Status == AmendmentStatus.Submitted)
                            && !related.Any(ba => ba.Status == AmendmentStatus.Rejected),
                        "Rejected"  => related.Any()
                            && !related.Any(ba => ba.Status == AmendmentStatus.Submitted)
                            && related.GroupBy(ba => ba.DepartmentID).Any(g => g.All(ba => ba.Status == AmendmentStatus.Rejected)),
                        _          => true
                    };
                }).ToList();
            }

            if (ShowOverdueOnly &&
                (string.IsNullOrEmpty(SelectedBAMainStatusTab) ||
                 SelectedBAMainStatusTab == "Submitted" ||
                 SelectedBAMainStatusTab == "Rejected" ||
                 SelectedBAMainStatusTab == "All"))
            {
                BudgetAmendmentsMain = BudgetAmendmentsMain
                    .Where(bam => bam.ExtendedDeadline.Date < DateTime.Today)
                    .ToList();
            }

            TotalBAMains = BudgetAmendmentsMain.Count;
            BAMainTotalPages = (int)Math.Ceiling(TotalBAMains / (double)BAMainResultsPerPage);
            BudgetAmendmentsMain = BudgetAmendmentsMain
                .Skip((BAMainCurrentPage - 1) * BAMainResultsPerPage)
                .Take(BAMainResultsPerPage)
                .ToList();

            Departments = await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync();

            DeptExtensionCounts = await _context.BADepartmentExtensions
                .GroupBy(e => e.BudgetAmendmentMainID)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }


        public async Task<IActionResult> OnGetAsync(int baMainPageNumber = 1, int baMainResultsPerPage = 10)
        {
            BAMainCurrentPage = baMainPageNumber;
            BAMainResultsPerPage = baMainResultsPerPage;

            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            if (!_authService.IsAdminRole(HttpContext))
                return Forbid();

            NewBudgetAmendmentMain = new BudgetAmendmentMain
            {
                StartDate = DateTime.Now,
                EndDate   = DateTime.Now.AddDays(30)
            };

            await LoadFormDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAddAmendmentMainAsync()
        {
            if (!_authService.IsAdminRole(HttpContext))
                return Forbid();

            if (string.IsNullOrWhiteSpace(NewBudgetAmendmentMain?.Name))
            {
                await LoadFormDataAsync();
                return Page();
            }

            NewBudgetAmendmentMain.CreatedAt = DateTime.Now;
            NewBudgetAmendmentMain.CreatedBy = _authService.GetAuthenticatedUserID(HttpContext);
            NewBudgetAmendmentMain.UpdatedAt = DateTime.Now;
            NewBudgetAmendmentMain.UpdatedBy = _authService.GetAuthenticatedUserID(HttpContext);
            NewBudgetAmendmentMain.ExtendedDeadline = NewBudgetAmendmentMain.EndDate;

            _context.BudgetAmendmentMain.Add(NewBudgetAmendmentMain);
            await _context.SaveChangesAsync();
            await UpdateUserActivityLogAsync(NewBudgetAmendmentMain.Name, ActivityType.Edited);
            TempData["SuccessMessage"] = $"Budget amendment \"{NewBudgetAmendmentMain.Name}\" created successfully.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExtendDeadlineAsync(int BudgetAmendmentMainID, DateTime ExtendDeadlineTo)
        {
            if (!_authService.IsAdminRole(HttpContext))
                return Forbid();

            var amendment = await _context.BudgetAmendmentMain.FindAsync(BudgetAmendmentMainID);

            if (amendment == null)
                return NotFound();

            if (ExtendDeadlineTo <= amendment.EndDate)
            {
                ModelState.AddModelError("", "Extended deadline must be after the current end date.");
                await LoadFormDataAsync();
                return Page();
            }

            SelectedDepartments ??= new List<string>();
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
            await UpdateUserActivityLogAsync(amendment.Name, ActivityType.Edited);
            TempData["SuccessMessage"] = $"Deadline for \"{amendment.Name}\" updated successfully.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAmendmentMainAsync(int id)
        {
            if (!_authService.IsAdminRole(HttpContext))
                return Forbid();

            var amendmentMain = await _context.BudgetAmendmentMain.FindAsync(id);
            if (amendmentMain == null)
                return NotFound();

            var relatedAmendments = _context.BudgetAmendments
                .Where(ba => ba.BudgetAmendmentMainID == id);

            if (relatedAmendments.Any(ba =>
                    ba.Status == AmendmentStatus.Submitted ||
                    ba.Status == AmendmentStatus.Approved ||
                    ba.Status == AmendmentStatus.Rejected))
            {
                TempData["ErrorMessage"] = $"Cannot delete \"{amendmentMain.Name}\" — it has submitted, approved, or rejected entries.";
                return RedirectToPage(new { SelectedBAMainStatusTab = "Unsubmitted" });
            }

            var deptExtensions = _context.BADepartmentExtensions
                .Where(e => e.BudgetAmendmentMainID == id);

            _context.BADepartmentExtensions.RemoveRange(deptExtensions);
            _context.BudgetAmendments.RemoveRange(relatedAmendments);
            _context.BudgetAmendmentMain.Remove(amendmentMain);

            await _context.SaveChangesAsync();
            await UpdateUserActivityLogAsync(amendmentMain.Name, ActivityType.Deleted);
            TempData["SuccessMessage"] = $"Budget amendment \"{amendmentMain.Name}\" deleted successfully.";

            return RedirectToPage(new { SelectedBAMainStatusTab = "Unsubmitted" });
        }

        public PaginationViewModel GetBAMainPagination()
        {
            var filters =
                $"&SelectedBAMainStatusTab={SelectedBAMainStatusTab}" +
                $"&SelectedFinancialYear={SelectedFinancialYear}" +
                $"&CustomStartDate={CustomStartDate?.ToString("yyyy-MM-dd")}" +
                $"&CustomEndDate={CustomEndDate?.ToString("yyyy-MM-dd")}" +
                $"&ShowOverdueOnly={ShowOverdueOnly}";

            return new PaginationViewModel
            {
                CurrentPage = BAMainCurrentPage,
                TotalPages = BAMainTotalPages,
                TotalRecords = TotalBAMains,
                ResultsPerPage = BAMainResultsPerPage,
                PageSizes = PageSizes,
                AriaLabel = "Budget amendment page navigation",
                PrevUrl = $"?baMainPageNumber={BAMainCurrentPage - 1}&baMainResultsPerPage={BAMainResultsPerPage}{filters}",
                NextUrl = $"?baMainPageNumber={BAMainCurrentPage + 1}&baMainResultsPerPage={BAMainResultsPerPage}{filters}",
                SizeChangeUrlTemplate = $"?baMainPageNumber=1&baMainResultsPerPage=__SIZE__{filters}"
            };
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