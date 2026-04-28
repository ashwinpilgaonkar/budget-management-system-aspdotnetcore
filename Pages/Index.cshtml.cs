using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ClosedXML.Excel;
using budget_management_system_aspdotnetcore.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Department = budget_management_system_aspdotnetcore.Entities.Department;
using budget_management_system_aspdotnetcore.Helpers;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class IndexModel(
        CasdbtestContext context,
        SpeedTypeService speedTypeService,
        UserService userService,
        IAuthenticationService authService) : PageModel
    {
        // ==============================================
        // Instance Variables -- HELPER
        // ==============================================
        #region HELPER
        private readonly CasdbtestContext _context = context;
        private readonly SpeedTypeService _speedTypeService = speedTypeService;
        private readonly UserService _userService = userService;
        private readonly IAuthenticationService _authService = authService;

        public int userID { get; set; } = 0;
        public string userRole { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string ActiveSortTable { get; set; } = "Department";

        [BindProperty(SupportsGet = true)]
        public string SortColumn { get; set; } = "DepartmentID";
        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; } = "asc";

        public List<int> PageSizes { get; set; } = PaginationViewModel.DefaultPageSizes;
        #endregion

        // ==============================================
        // Instance Variables -- BUDGET AMENDMENTS
        // ==============================================
        #region BUDGET AMENDMENTS

        public IEnumerable<SpeedType> SpeedTypes { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedBudgetAmendmentMainID { get; set; }

        public List<BudgetAmendmentMain> BudgetAmendmentsMain { get; set; }
        public List<BudgetAmendmentMain> BudgetAmendmentsMainAll { get; set; }

        public List<BudgetAmendment> BudgetAmendmentsAll { get; set; }

        public List<BudgetAmendment> BudgetAmendments { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? EditingBudgetAmendmentID { get; set; }

        [BindProperty]
        public BudgetAmendment NewBudgetAmendment { get; set; }

        public int BudgetAmendmentCurrentPage { get; set; } = 1;
        public int BudgetAmendmentResultsPerPage { get; set; } = 10;
        public int BudgetAmendmentTotalPages { get; set; }

        public int TotalBudgetAmendments { get; set; }

        public int BAMainCurrentPage { get; set; } = 1;
        public int BAMainResultsPerPage { get; set; } = 10;
        public int BAMainTotalPages { get; set; }
        public int TotalBAMains { get; set; }

        [BindProperty(SupportsGet = true)]
        public string BudgetAmendmentSearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        [Required]
        public int SourceSpeedtype { get; set; }

        [BindProperty(SupportsGet = true)]
        [Required]
        public int DestinationSpeedtype { get; set; }

        public DateTime BudgetAmendmentMainStartDate { get; set; }
        public DateTime BudgetAmendmentMainEndDate { get; set; }

        [BindProperty]
        [Range(0.01, double.MaxValue, ErrorMessage = "Transfer amount must be greater than zero")]
        public double TransferAmount { get; set; }

        public List<Department> DepartmentsUserIsResponsibleFor { get; set; }
        [BindProperty(SupportsGet = true)]
        public int SelectedDepartmentID { get; set; } = 0;

        [BindProperty(SupportsGet = true)]
        public string SelectedStatusTab { get; set; } = AmendmentStatus.Submitted.ToString();

        [BindProperty(SupportsGet = true)]
        public string? SelectedBAMainStatusTab { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool ShowOverdueOnly { get; set; } = false;

        [BindProperty(SupportsGet = true)]
        public bool BAMainCollapsed { get; set; } = false;

        [BindProperty(SupportsGet = true)]
        public bool BADeptCollapsed { get; set; } = false;

        [BindProperty(SupportsGet = true)]
        public int? SelectedCreatedBy { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? CreatedFromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? CreatedToDate { get; set; }

        public List<SelectListItem> CreatedByUsers { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedEditedBy { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EditedFromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EditedToDate { get; set; }

        public List<SelectListItem> EditedByUsers { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedUpdatedBy { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? UpdatedFromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? UpdatedToDate { get; set; }

        public List<SelectListItem> UpdatedByUsers { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedFinancialYear { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? CustomStartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? CustomEndDate { get; set; }

        public List<string> FinancialYearOptions => FinancialYearHelper.GetOptions();

        public Dictionary<string, int> BAMainTabCounts { get; set; } = new();

        public FilterStateViewModel FilterState => new()
        {
            SelectedDepartmentID = SelectedDepartmentID,
            SelectedStatusTab = SelectedStatusTab,
            SelectedBAMainStatusTab = SelectedBAMainStatusTab,
            SelectedBudgetAmendmentMainID = SelectedBudgetAmendmentMainID,
            SelectedFinancialYear = SelectedFinancialYear,
            CustomStartDate = CustomStartDate,
            CustomEndDate = CustomEndDate
        };

        public FilterStateFullViewModel FilterStateFull => new()
        {
            SelectedDepartmentID = SelectedDepartmentID,
            SelectedStatusTab = SelectedStatusTab,
            SelectedBAMainStatusTab = SelectedBAMainStatusTab,
            SelectedBudgetAmendmentMainID = SelectedBudgetAmendmentMainID,
            SelectedFinancialYear = SelectedFinancialYear,
            CustomStartDate = CustomStartDate,
            CustomEndDate = CustomEndDate,
            BudgetAmendmentSearchTerm = BudgetAmendmentSearchTerm,
            SelectedCreatedBy = SelectedCreatedBy,
            CreatedFromDate = CreatedFromDate,
            CreatedToDate = CreatedToDate,
            SelectedEditedBy = SelectedEditedBy,
            EditedFromDate = EditedFromDate,
            EditedToDate = EditedToDate,
            SelectedUpdatedBy = SelectedUpdatedBy,
            UpdatedFromDate = UpdatedFromDate,
            UpdatedToDate = UpdatedToDate
        };

        #endregion

        // ==============================================
        //                 DATA LOADING
        // ==============================================
        #region DATA LOADING

        private void SetDefaultFinancialYearRange()
        {
            var today = DateTime.Today;
            int startYear = today.Month >= 7 ? today.Year : today.Year - 1;

            if (!CustomStartDate.HasValue)
                CustomStartDate = new DateTime(startYear, 7, 1);

            if (!CustomEndDate.HasValue)
                CustomEndDate = new DateTime(startYear + 1, 6, 30);
        }

        public async Task LoadFormDataAsync()
        {
            userRole = _authService.GetUserRole(HttpContext);
            userID = _authService.GetAuthenticatedUserID(HttpContext);

            if (string.IsNullOrEmpty(SelectedBAMainStatusTab))
                SelectedBAMainStatusTab = userRole == RoleConstants.AFOIdString ? "Unsubmitted" : "Submitted";

            SetDefaultFinancialYearRange();


            SpeedTypes = await _speedTypeService.GetSpeedTypesAsync();

            if (string.IsNullOrEmpty(SelectedFinancialYear) && FinancialYearOptions.Any())
                SelectedFinancialYear = FinancialYearOptions.First();

            var (startDate, endDate) = FinancialYearHelper.GetDateRange(SelectedFinancialYear, CustomStartDate, CustomEndDate);

            BudgetAmendmentsMain = await _context.BudgetAmendmentMain
            .Where(ba => ba.ExtendedDeadline >= startDate && ba.ExtendedDeadline <= endDate)
            .OrderByDescending(ba => ba.CreatedAt)
            .ToListAsync();

            BudgetAmendmentsAll = await _context.BudgetAmendments
                .Where(ba => ba.BudgetAmendmentMain.ExtendedDeadline >= startDate && ba.BudgetAmendmentMain.ExtendedDeadline <= endDate)
                .ToListAsync();

            BudgetAmendmentsMainAll = BudgetAmendmentsMain;

            BAMainTabCounts = new Dictionary<string, int>
            {
                ["All"] = BudgetAmendmentsMainAll.Count,
                ["Unsubmitted"] = BudgetAmendmentsMainAll.Count(bam =>
                {
                    var rel = BudgetAmendmentsAll.Where(ba => ba.BudgetAmendmentMainID == bam.BudgetAmendmentMainID).ToList();
                    return !rel.Any(ba => ba.Status == AmendmentStatus.Submitted
                                      || ba.Status == AmendmentStatus.Approved
                                      || ba.Status == AmendmentStatus.Rejected);
                }),
                ["Submitted"] = BudgetAmendmentsMainAll.Count(bam =>
                    BudgetAmendmentsAll.Any(ba => ba.BudgetAmendmentMainID == bam.BudgetAmendmentMainID && ba.Status == AmendmentStatus.Submitted)),
                ["Approved"] = BudgetAmendmentsMainAll.Count(bam =>
                {
                    var rel = BudgetAmendmentsAll.Where(ba => ba.BudgetAmendmentMainID == bam.BudgetAmendmentMainID).ToList();
                    return rel.Any(ba => ba.Status == AmendmentStatus.Approved)
                        && !rel.Any(ba => ba.Status == AmendmentStatus.Submitted)
                        && !rel.Any(ba => ba.Status == AmendmentStatus.Rejected);
                }),
                ["Rejected"] = BudgetAmendmentsMainAll.Count(bam =>
                {
                    var rel = BudgetAmendmentsAll.Where(ba => ba.BudgetAmendmentMainID == bam.BudgetAmendmentMainID).ToList();
                    return rel.Any()
                        && !rel.Any(ba => ba.Status == AmendmentStatus.Submitted)
                        && rel.GroupBy(ba => ba.DepartmentID).Any(g => g.All(ba => ba.Status == AmendmentStatus.Rejected));
                })
            };

            if (!string.IsNullOrEmpty(SelectedBAMainStatusTab))
            {
                BudgetAmendmentsMain = BudgetAmendmentsMain.Where(bam =>
                {
                    var related = BudgetAmendmentsAll.Where(ba => ba.BudgetAmendmentMainID == bam.BudgetAmendmentMainID).ToList();
                    return SelectedBAMainStatusTab switch
                    {
                        "Unsubmitted" => !related.Any(ba => ba.Status == AmendmentStatus.Submitted
                                                         || ba.Status == AmendmentStatus.Approved
                                                         || ba.Status == AmendmentStatus.Rejected),
                        "Submitted" => related.Any(ba => ba.Status == AmendmentStatus.Submitted),
                        "Approved"  => related.Any(ba => ba.Status == AmendmentStatus.Approved)
                            && !related.Any(ba => ba.Status == AmendmentStatus.Submitted)
                            && !related.Any(ba => ba.Status == AmendmentStatus.Rejected),
                        "Rejected"  => related.Any()
                            && !related.Any(ba => ba.Status == AmendmentStatus.Submitted)
                            && related.GroupBy(ba => ba.DepartmentID).Any(g => g.All(ba => ba.Status == AmendmentStatus.Rejected)),
                        _           => true
                    };
                }).ToList();
            }

            if (ShowOverdueOnly &&
                (string.IsNullOrEmpty(SelectedBAMainStatusTab) ||
                 SelectedBAMainStatusTab == "Unsubmitted" ||
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

            var amendmentQuery = _context.BudgetAmendments.AsQueryable();

            if (!string.IsNullOrEmpty(BudgetAmendmentSearchTerm))
            {
                amendmentQuery = amendmentQuery.Where(s => s.CategoryName.Contains(BudgetAmendmentSearchTerm)
                || s.AdjustmentDetail.ToString().Contains(BudgetAmendmentSearchTerm)
                || s.SpeedType.Code.ToString().Contains(BudgetAmendmentSearchTerm)
                || s.FundCode.ToString().Contains(BudgetAmendmentSearchTerm)
                || s.DepartmentID.ToString().Contains(BudgetAmendmentSearchTerm)
                || s.ProgramCode.ToString().Contains(BudgetAmendmentSearchTerm)
                || s.ClassCode.ToString().Contains(BudgetAmendmentSearchTerm)
                || s.AcctDescription.ToString().Contains(BudgetAmendmentSearchTerm)
                || s.BudgetCode.ToString().Contains(BudgetAmendmentSearchTerm)
                || s.PositionNumber.ToString().Contains(BudgetAmendmentSearchTerm)
                || s.AmountIncrease.ToString().Contains(BudgetAmendmentSearchTerm)
                || s.AmountDecrease.ToString().Contains(BudgetAmendmentSearchTerm));
            }

            if (!string.IsNullOrEmpty(SelectedStatusTab) && Enum.TryParse<AmendmentStatus>(SelectedStatusTab, out var parsedStatus))
            {
                amendmentQuery = amendmentQuery.Where(b => b.Status == parsedStatus);
            }

            if (SelectedDepartmentID != 0)
            {
                amendmentQuery = amendmentQuery.Where(b => b.DepartmentID == SelectedDepartmentID);
            }

            if (SelectedBudgetAmendmentMainID.HasValue)
            {
                amendmentQuery = amendmentQuery.Where(b => b.BudgetAmendmentMainID == SelectedBudgetAmendmentMainID.Value);
            }

            if (!string.IsNullOrEmpty(SortColumn) && ActiveSortTable == "AmendmentHistory")
            {
                amendmentQuery = SortColumn switch
                {
                    "AmountIncrease" => SortOrder == "asc"
                        ? amendmentQuery.OrderBy(e => e.AmountIncrease)
                        : amendmentQuery.OrderByDescending(e => e.AmountIncrease),
                    "AmountDecrease" => SortOrder == "asc"
                        ? amendmentQuery.OrderBy(e => e.AmountDecrease)
                        : amendmentQuery.OrderByDescending(e => e.AmountDecrease),
                    "FundCode" => SortOrder == "asc"
                        ? amendmentQuery.OrderBy(e => e.FundCode)
                        : amendmentQuery.OrderByDescending(e => e.FundCode),
                    "ProgramCode" => SortOrder == "asc"
                        ? amendmentQuery.OrderBy(e => e.ProgramCode)
                        : amendmentQuery.OrderByDescending(e => e.ProgramCode),
                    "ClassCode" => SortOrder == "asc"
                        ? amendmentQuery.OrderBy(e => e.ClassCode)
                        : amendmentQuery.OrderByDescending(e => e.ClassCode),
                    _ => SortOrder == "asc"
                        ? amendmentQuery.OrderBy(e => EF.Property<string>(e, SortColumn))
                        : amendmentQuery.OrderByDescending(e => EF.Property<string>(e, SortColumn))
                };
            }

            CreatedByUsers = await amendmentQuery
                .Where(b => b.CreatedBy != null)
                .Select(b => new
                {
                    b.CreatedBy,
                    b.CreatedByUser.Email
                })
                .Distinct()
                .Select(u => new SelectListItem
                {
                    Value = u.CreatedBy.ToString(),
                    Text = u.Email
                })
                .ToListAsync();

            EditedByUsers = await _context.BudgetAmendments
                .Where(b => b.EditedBy != null)
                .Select(b => new { b.EditedBy, b.EditedByUser.Email })
                .Distinct()
                .Select(u => new SelectListItem
                {
                    Value = u.EditedBy.ToString(),
                    Text = u.Email
                }).ToListAsync();

            UpdatedByUsers = await _context.BudgetAmendments
                .Where(b => b.UpdatedBy != null)
                .Select(b => new { b.UpdatedBy, b.UpdatedByUser.Email })
                .Distinct()
                .Select(u => new SelectListItem
                {
                    Value = u.UpdatedBy.ToString(),
                    Text = u.Email
                }).ToListAsync();

            if (!CreatedFromDate.HasValue)
                CreatedFromDate = DateTime.MinValue;

            if (!CreatedToDate.HasValue)
                CreatedToDate = DateTime.Today.AddDays(1).AddTicks(-1);

            if (!EditedFromDate.HasValue)
                EditedFromDate = DateTime.MinValue;

            if (!EditedToDate.HasValue)
                EditedToDate = DateTime.Today.AddDays(1).AddTicks(-1);

            if (!UpdatedFromDate.HasValue)
                UpdatedFromDate = DateTime.MinValue;

            if (!UpdatedToDate.HasValue)
                UpdatedToDate = DateTime.Today.AddDays(1).AddTicks(-1);

            amendmentQuery = amendmentQuery.Where(a => a.CreatedAt >= CreatedFromDate && a.CreatedAt <= CreatedToDate);
            amendmentQuery = amendmentQuery.Where(a => a.EditedAt >= EditedFromDate && a.EditedAt <= EditedToDate);
            amendmentQuery = amendmentQuery.Where(a => a.UpdatedAt >= UpdatedFromDate && a.UpdatedAt <= UpdatedToDate);

            if (SelectedCreatedBy.HasValue)
                amendmentQuery = amendmentQuery.Where(b => b.CreatedBy == SelectedCreatedBy);

            if (SelectedEditedBy.HasValue)
                amendmentQuery = amendmentQuery.Where(b => b.EditedBy == SelectedEditedBy);

            if (SelectedUpdatedBy.HasValue)
                amendmentQuery = amendmentQuery.Where(b => b.UpdatedBy == SelectedUpdatedBy);

            BudgetAmendments = await amendmentQuery
            .Include(a => a.CreatedByUser)
            .Include(a => a.EditedByUser)
            .Include(a => a.UpdatedByUser)
            .Skip((BudgetAmendmentCurrentPage - 1) * BudgetAmendmentResultsPerPage)
            .Take(BudgetAmendmentResultsPerPage)
            .ToListAsync();

            TotalBudgetAmendments = await amendmentQuery.CountAsync();
            BudgetAmendmentTotalPages = (int)Math.Ceiling(TotalBudgetAmendments / (double)BudgetAmendmentResultsPerPage);

            var currentMainAmendment = await _context.BudgetAmendmentMain
                .FirstOrDefaultAsync(ba => ba.BudgetAmendmentMainID == SelectedBudgetAmendmentMainID);

            if (currentMainAmendment != null)
            {
                BudgetAmendmentMainStartDate = currentMainAmendment.StartDate;
                BudgetAmendmentMainEndDate = currentMainAmendment.ExtendedDeadline;
            }

            var userId = _authService.GetAuthenticatedUserID(HttpContext);
            DepartmentsUserIsResponsibleFor = _userService.GetDepartmentsResponsibleForLoggedInUser(userId).ToList();
        }


        public async Task<IActionResult> OnGetAsync(
            int amendmentPageNumber = 1,
            int amendmentResultsPerPage = 10,
            int baMainPageNumber = 1,
            int baMainResultsPerPage = 10)
        {
            BudgetAmendmentCurrentPage = amendmentPageNumber;
            BudgetAmendmentResultsPerPage = amendmentResultsPerPage;
            BAMainCurrentPage = baMainPageNumber;
            BAMainResultsPerPage = baMainResultsPerPage;

            if (!_authService.IsAuthenticated(HttpContext))
            {
                return RedirectToPage("/Login");
            }

            await LoadFormDataAsync();
            return Page();
        }

        public async Task OnGetSortColumn(string table, string column, string order)
        {
            ActiveSortTable = table;
            SortColumn = column;
            SortOrder = order;

            await LoadFormDataAsync();
        }

        public string SortOrderForColumn(string column)
        {
            return SortColumn == column && SortOrder == "asc" ? "desc" : "asc";
        }

        public string GetStatusClass(AmendmentStatus status) => status switch
        {
            AmendmentStatus.Approved => "text-success",
            AmendmentStatus.Rejected => "text-danger",
            AmendmentStatus.Draft    => "text-muted",
            _                        => ""
        };

        public string GetSortIcon(string column)
        {
            if (SortColumn == column)
            {
                return SortOrder == "asc" ? "fa-arrow-up" : "fa-arrow-down";
            }
            return "fa-sort";
        }

        public PaginationViewModel GetBAMainPagination()
        {
            var filters =
                $"&SelectedBAMainStatusTab={SelectedBAMainStatusTab}" +
                $"&SelectedFinancialYear={SelectedFinancialYear}" +
                $"&CustomStartDate={CustomStartDate?.ToString("yyyy-MM-dd")}" +
                $"&CustomEndDate={CustomEndDate?.ToString("yyyy-MM-dd")}" +
                $"&SelectedDepartmentID={SelectedDepartmentID}" +
                $"&SelectedStatusTab={SelectedStatusTab}" +
                $"&SelectedBudgetAmendmentMainID={SelectedBudgetAmendmentMainID}" +
                $"&amendmentPageNumber={BudgetAmendmentCurrentPage}" +
                $"&amendmentResultsPerPage={BudgetAmendmentResultsPerPage}" +
                $"&ShowOverdueOnly={ShowOverdueOnly}" +
                $"&BAMainCollapsed={BAMainCollapsed}" +
                $"&BADeptCollapsed={BADeptCollapsed}";

            return new PaginationViewModel
            {
                CurrentPage = BAMainCurrentPage,
                TotalPages = BAMainTotalPages,
                TotalRecords = TotalBAMains,
                ResultsPerPage = BAMainResultsPerPage,
                PageSizes = PageSizes,
                AriaLabel = "Budget amendment main page navigation",
                PrevUrl = $"?baMainPageNumber={BAMainCurrentPage - 1}&baMainResultsPerPage={BAMainResultsPerPage}{filters}",
                NextUrl = $"?baMainPageNumber={BAMainCurrentPage + 1}&baMainResultsPerPage={BAMainResultsPerPage}{filters}",
                SizeChangeUrlTemplate = $"?baMainPageNumber=1&baMainResultsPerPage=__SIZE__{filters}"
            };
        }

        #endregion

        // ==============================================
        //           BUDGET AMENDMENT METHODS
        // ==============================================
        #region BUDGET AMENDMENT METHODS

        public async Task<IActionResult> OnPostAddAmendmentAsync()
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            if (string.IsNullOrWhiteSpace(NewBudgetAmendment.CategoryName)
                || string.IsNullOrWhiteSpace(NewBudgetAmendment.AdjustmentDetail)
                || string.IsNullOrWhiteSpace(NewBudgetAmendment.AcctDescription)
                || string.IsNullOrWhiteSpace(NewBudgetAmendment.BudgetCode)
                || string.IsNullOrWhiteSpace(NewBudgetAmendment.PositionNumber)
                || TransferAmount <= 0)
            {
                TempData["ErrorMessage"] = "All fields are required to create a Budget Amendment entry.";
                return RedirectToPage(new
                {
                    SelectedDepartmentID,
                    SelectedBudgetAmendmentMainID,
                    SelectedBAMainStatusTab,
                    SelectedStatusTab,
                    SelectedFinancialYear,
                    CustomStartDate = CustomStartDate?.ToString("yyyy-MM-dd"),
                    CustomEndDate = CustomEndDate?.ToString("yyyy-MM-dd")
                });
            }

            var hasBlockingEntries = await _context.BudgetAmendments.AnyAsync(ba =>
                ba.BudgetAmendmentMainID == NewBudgetAmendment.BudgetAmendmentMainID
                && ba.DepartmentID == NewBudgetAmendment.DepartmentID
                && (ba.Status == AmendmentStatus.Submitted
                    || ba.Status == AmendmentStatus.Approved
                    || ba.Status == AmendmentStatus.Rejected));

            if (hasBlockingEntries)
            {
                TempData["ErrorMessage"] = "Cannot create new drafts: this department already has entries that are either Submitted, Approved, or Need review.";
                return RedirectToPage(new
                {
                    SelectedDepartmentID,
                    SelectedBudgetAmendmentMainID,
                    SelectedStatusTab,
                    SelectedFinancialYear,
                    CustomStartDate = CustomStartDate?.ToString("yyyy-MM-dd"),
                    CustomEndDate = CustomEndDate?.ToString("yyyy-MM-dd")
                });
            }

            var transactionId = Guid.NewGuid();

            var budgetAmendment1 = new BudgetAmendment
            {
                CategoryName = NewBudgetAmendment.CategoryName,
                AdjustmentDetail = NewBudgetAmendment.AdjustmentDetail,
                FundCode = NewBudgetAmendment.FundCode,
                DepartmentID = NewBudgetAmendment.DepartmentID,
                ProgramCode = NewBudgetAmendment.ProgramCode,
                ClassCode = NewBudgetAmendment.ClassCode,
                AcctDescription = NewBudgetAmendment.AcctDescription,
                BudgetCode = NewBudgetAmendment.BudgetCode,
                PositionNumber = NewBudgetAmendment.PositionNumber,
                TransactionId = transactionId,

                SpeedTypeId = DestinationSpeedtype,
                AmountIncrease = TransferAmount,
                AmountDecrease = 0,

                CreatedBy = _authService.GetAuthenticatedUserID(HttpContext),
                UpdatedBy = _authService.GetAuthenticatedUserID(HttpContext),
                EditedBy = _authService.GetAuthenticatedUserID(HttpContext),
                EditedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                BudgetAmendmentMainID = NewBudgetAmendment.BudgetAmendmentMainID
            };

            var budgetAmendment2 = new BudgetAmendment
            {
                CategoryName = NewBudgetAmendment.CategoryName,
                AdjustmentDetail = NewBudgetAmendment.AdjustmentDetail,
                FundCode = NewBudgetAmendment.FundCode,
                DepartmentID = NewBudgetAmendment.DepartmentID,
                ProgramCode = NewBudgetAmendment.ProgramCode,
                ClassCode = NewBudgetAmendment.ClassCode,
                AcctDescription = NewBudgetAmendment.AcctDescription,
                BudgetCode = NewBudgetAmendment.BudgetCode,
                PositionNumber = NewBudgetAmendment.PositionNumber,
                TransactionId = transactionId,

                SpeedTypeId = SourceSpeedtype,
                AmountIncrease = 0,
                AmountDecrease = TransferAmount,

                CreatedBy = _authService.GetAuthenticatedUserID(HttpContext),
                UpdatedBy = _authService.GetAuthenticatedUserID(HttpContext),
                EditedBy = _authService.GetAuthenticatedUserID(HttpContext),
                EditedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                BudgetAmendmentMainID = NewBudgetAmendment.BudgetAmendmentMainID
            };

            _context.BudgetAmendments.Add(budgetAmendment1);
            _context.BudgetAmendments.Add(budgetAmendment2);

            SelectedStatusTab = AmendmentStatus.Draft.ToString();

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Entry \"{NewBudgetAmendment.CategoryName}\" added successfully.";

            return RedirectToPage(new
            {
                SelectedDepartmentID,
                SelectedBudgetAmendmentMainID,
                SelectedBAMainStatusTab,
                SelectedStatusTab,
                SelectedFinancialYear,
                CustomStartDate = CustomStartDate?.ToString("yyyy-MM-dd"),
                CustomEndDate = CustomEndDate?.ToString("yyyy-MM-dd")
            });
        }

        public async Task<IActionResult> OnPostEditAmendmentAsync(int id)
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            var amendment = await _context.BudgetAmendments.FindAsync(id);

            if (amendment != null && (amendment.Status == AmendmentStatus.Draft
                || amendment.Status == AmendmentStatus.Rejected
                || (amendment.Status == AmendmentStatus.Submitted && _authService.IsAdminRole(HttpContext))))
            {
                EditingBudgetAmendmentID = id;

                var relatedAmendments = await _context.BudgetAmendments
                    .Where(a => a.TransactionId == amendment.TransactionId)
                    .ToListAsync();

                if (relatedAmendments.Count == 2)
                {
                    // Identify source and destination amendments in the pair 
                    var sourceAmendment = relatedAmendments.FirstOrDefault(a => a.AmountIncrease == 0);
                    var destinationAmendment = relatedAmendments.FirstOrDefault(a => a.AmountDecrease == 0);

                    if (sourceAmendment != null && destinationAmendment != null)
                    {
                        SourceSpeedtype = sourceAmendment.SpeedTypeId;
                        DestinationSpeedtype = destinationAmendment.SpeedTypeId;
                    }
                }
            }

            return RedirectToPage(new
            {
                SelectedDepartmentID,
                SelectedBudgetAmendmentMainID,
                SelectedStatusTab,
                SelectedBAMainStatusTab,
                SelectedFinancialYear,
                CustomStartDate = CustomStartDate?.ToString("yyyy-MM-dd"),
                CustomEndDate = CustomEndDate?.ToString("yyyy-MM-dd"),
                EditingBudgetAmendmentID,
                SourceSpeedtype,
                DestinationSpeedtype
            });
        }

        public async Task<IActionResult> OnPostCancelEditAmendmentAsync(int id)
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            EditingBudgetAmendmentID = 0;

            return RedirectToPage(new
            {
                SelectedDepartmentID,
                SelectedBudgetAmendmentMainID,
                SelectedStatusTab,
                SelectedBAMainStatusTab,
                SelectedFinancialYear,
                CustomStartDate = CustomStartDate?.ToString("yyyy-MM-dd"),
                CustomEndDate = CustomEndDate?.ToString("yyyy-MM-dd"),
                EditingBudgetAmendmentID
            });
        }

        public async Task<IActionResult> OnPostSaveAmendmentAsync()
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            var amendment = await _context.BudgetAmendments.FindAsync(NewBudgetAmendment.BudgetAmendmentID);

            if (amendment != null)
            {
                var canEdit = amendment.Status == AmendmentStatus.Draft
                    || amendment.Status == AmendmentStatus.Rejected
                    || (amendment.Status == AmendmentStatus.Submitted && _authService.IsAdminRole(HttpContext));

                if (!canEdit)
                    return Forbid();

                var relatedAmendments = await _context.BudgetAmendments
                    .Where(a => a.TransactionId == amendment.TransactionId)
                    .ToListAsync();

                if (relatedAmendments.Count == 2)
                {
                    // Identify source and destination amendments in the pair 
                    var sourceAmendment = relatedAmendments.FirstOrDefault(a => a.SpeedTypeId == SourceSpeedtype);
                    var destinationAmendment = relatedAmendments.FirstOrDefault(a => a.SpeedTypeId == DestinationSpeedtype);

                    if (sourceAmendment == null || destinationAmendment == null)
                    { 
                        return NotFound();
                    }

                    foreach (var relatedAmendment in relatedAmendments)
                    {
                        relatedAmendment.CategoryName = NewBudgetAmendment.CategoryName;
                        relatedAmendment.AdjustmentDetail = NewBudgetAmendment.AdjustmentDetail;
                        relatedAmendment.FundCode = NewBudgetAmendment.FundCode;
                        relatedAmendment.DepartmentID = NewBudgetAmendment.DepartmentID;
                        relatedAmendment.ProgramCode = NewBudgetAmendment.ProgramCode;
                        relatedAmendment.ClassCode = NewBudgetAmendment.ClassCode;
                        relatedAmendment.AcctDescription = NewBudgetAmendment.AcctDescription;
                        relatedAmendment.BudgetCode = NewBudgetAmendment.BudgetCode;
                        relatedAmendment.PositionNumber = NewBudgetAmendment.PositionNumber;

                        relatedAmendment.EditedAt = DateTime.Now;
                        relatedAmendment.EditedBy = _authService.GetAuthenticatedUserID(HttpContext);
                    }

                    sourceAmendment.AmountIncrease = NewBudgetAmendment.AmountDecrease;
                    sourceAmendment.AmountDecrease = NewBudgetAmendment.AmountIncrease;

                    destinationAmendment.AmountIncrease = NewBudgetAmendment.AmountIncrease;
                    destinationAmendment.AmountDecrease = NewBudgetAmendment.AmountDecrease;

                    await _context.SaveChangesAsync();
                }
                await UpdateUserActivityLogAsync(NewBudgetAmendment.CategoryName, ActivityType.Edited);
                TempData["SuccessMessage"] = $"Entry \"{NewBudgetAmendment.CategoryName}\" saved successfully.";
            }

            return RedirectToPage(new
            {
                SelectedDepartmentID,
                SelectedBudgetAmendmentMainID,
                SelectedStatusTab,
                SelectedFinancialYear,
                CustomStartDate = CustomStartDate?.ToString("yyyy-MM-dd"),
                CustomEndDate = CustomEndDate?.ToString("yyyy-MM-dd")
            });
        }

        public async Task<IActionResult> OnPostDeleteAmendmentAsync(int id)
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            var amendment = await _context.BudgetAmendments.FindAsync(id);

            if (amendment == null)
            {
                return NotFound();
            }

            if (amendment.Status == AmendmentStatus.Draft
                || amendment.Status == AmendmentStatus.Rejected
                || (amendment.Status == AmendmentStatus.Submitted && _authService.IsAdminRole(HttpContext)))
            {
                var categoryName = amendment.CategoryName;

                var transactionId = amendment.TransactionId;

                var relatedAmendments = _context.BudgetAmendments
                    .Where(a => a.TransactionId == transactionId);

                _context.BudgetAmendments.RemoveRange(relatedAmendments);
                await _context.SaveChangesAsync();

                await UpdateUserActivityLogAsync(categoryName, ActivityType.Deleted);
                TempData["SuccessMessage"] = $"Entry \"{categoryName}\" deleted.";
            }

            return RedirectToPage(new
            {
                SelectedDepartmentID,
                SelectedBudgetAmendmentMainID,
                SelectedStatusTab,
                SelectedFinancialYear,
                CustomStartDate = CustomStartDate?.ToString("yyyy-MM-dd"),
                CustomEndDate = CustomEndDate?.ToString("yyyy-MM-dd")
            });
        }

        public async Task<IActionResult> OnPostApproveCategoryAsync()
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            if (!_authService.IsAdminRole(HttpContext))
                return Forbid();

            if (SelectedDepartmentID == 0)
                return BadRequest("Invalid department");

            var amendments = await _context.BudgetAmendments
                .Where(a => a.DepartmentID == SelectedDepartmentID && a.Status == AmendmentStatus.Submitted)
                .ToListAsync();

            if (!AllCategoryNetChangesAreZero(amendments))
            {
                TempData["ErrorMessage"] = "Cannot approve these records. Please ensure that the amounts increase/decrease total to zero.";
                return RedirectToPage(new
                {
                    SelectedDepartmentID,
                    SelectedBudgetAmendmentMainID,
                    SelectedStatusTab,
                    SelectedFinancialYear,
                    CustomStartDate = CustomStartDate?.ToString("yyyy-MM-dd"),
                    CustomEndDate = CustomEndDate?.ToString("yyyy-MM-dd")
                });
            }

            var categoryName = "";

            foreach (var amendment in amendments)
            {
                amendment.Status = AmendmentStatus.Approved;
                amendment.UpdatedBy = _authService.GetAuthenticatedUserID(HttpContext);
                amendment.UpdatedAt = DateTime.Now;

                categoryName = amendment.CategoryName;
                await UpdateUserActivityLogAsync(amendment.CategoryName, ActivityType.Approved);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "All entries approved.";
            return RedirectToPage(new
            {
                SelectedDepartmentID,
                SelectedBudgetAmendmentMainID,
                SelectedStatusTab,
                SelectedFinancialYear,
                CustomStartDate = CustomStartDate?.ToString("yyyy-MM-dd"),
                CustomEndDate = CustomEndDate?.ToString("yyyy-MM-dd")
            });
        }

        public async Task<IActionResult> OnPostRejectCategoryAsync()
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            if (!_authService.IsAdminRole(HttpContext))
                return Forbid();

            if (SelectedDepartmentID == 0)
                return BadRequest("Invalid department");

            var amendments = await _context.BudgetAmendments
                .Where(a => a.DepartmentID == SelectedDepartmentID && a.Status == AmendmentStatus.Submitted)
                .ToListAsync();
            var categoryName = "";

            foreach (var amendment in amendments)
            {
                amendment.Status = AmendmentStatus.Rejected;
                amendment.UpdatedBy = _authService.GetAuthenticatedUserID(HttpContext);
                amendment.UpdatedAt = DateTime.Now;

                categoryName = amendment.CategoryName;
                await UpdateUserActivityLogAsync(categoryName, ActivityType.Rejected);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "All entries rejected.";
            return RedirectToPage(new
            {
                SelectedDepartmentID,
                SelectedBudgetAmendmentMainID,
                SelectedStatusTab,
                SelectedFinancialYear,
                CustomStartDate = CustomStartDate?.ToString("yyyy-MM-dd"),
                CustomEndDate = CustomEndDate?.ToString("yyyy-MM-dd")
            });
        }

        public async Task<IActionResult> OnPostRevertCategoryAsync()
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            if (!_authService.IsAdminRole(HttpContext))
                return Forbid();

            if (SelectedDepartmentID == 0)
                return BadRequest("Invalid department");

            var amendments = await _context.BudgetAmendments.Where(a => a.DepartmentID == SelectedDepartmentID && (a.Status == AmendmentStatus.Approved || a.Status == AmendmentStatus.Rejected)).ToListAsync();

            var rejectedAmendments = amendments.Where(a => a.Status == AmendmentStatus.Rejected).ToList();
            if (rejectedAmendments.Any() && !AllCategoryNetChangesAreZero(rejectedAmendments))
            {
                TempData["ErrorMessage"] = "Cannot resubmit these records for review. Please ensure that the amounts increase/decrease total to zero.";
                return RedirectToPage(new
                {
                    SelectedDepartmentID,
                    SelectedBudgetAmendmentMainID,
                    SelectedStatusTab,
                    SelectedFinancialYear,
                    CustomStartDate = CustomStartDate?.ToString("yyyy-MM-dd"),
                    CustomEndDate = CustomEndDate?.ToString("yyyy-MM-dd")
                });
            }

            var categoryName = "";

            foreach (var amendment in amendments)
            {
                amendment.Status = AmendmentStatus.Submitted;
                amendment.UpdatedBy = _authService.GetAuthenticatedUserID(HttpContext);
                amendment.UpdatedAt = DateTime.Now;

                categoryName = amendment.CategoryName;
                await UpdateUserActivityLogAsync(categoryName, ActivityType.Reverted);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Entries reverted to Submitted: Pending review.";
            return RedirectToPage(new
            {
                SelectedDepartmentID,
                SelectedBudgetAmendmentMainID,
                SelectedStatusTab,
                SelectedFinancialYear,
                CustomStartDate = CustomStartDate?.ToString("yyyy-MM-dd"),
                CustomEndDate = CustomEndDate?.ToString("yyyy-MM-dd")
            });
        }


        public async Task<IActionResult> OnPostSubmitCategoryAsync()
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            var amendments = await _context.BudgetAmendments.Where(a =>
                a.Status == AmendmentStatus.Draft
                && a.BudgetAmendmentMainID == SelectedBudgetAmendmentMainID
                && a.DepartmentID == SelectedDepartmentID).ToListAsync();

            if (!AllCategoryNetChangesAreZero(amendments))
            {
                TempData["ErrorMessage"] = "Cannot submit these records for review. Please ensure that the amounts increase/decrease total to zero.";
                return RedirectToPage(new
                {
                    SelectedDepartmentID,
                    SelectedBudgetAmendmentMainID,
                    SelectedStatusTab,
                    SelectedFinancialYear,
                    CustomStartDate = CustomStartDate?.ToString("yyyy-MM-dd"),
                    CustomEndDate = CustomEndDate?.ToString("yyyy-MM-dd")
                });
            }

            var categoryName = "";

            foreach (var amendment in amendments)
            {
                amendment.Status = AmendmentStatus.Submitted;
                amendment.UpdatedBy = _authService.GetAuthenticatedUserID(HttpContext);
                amendment.UpdatedAt = DateTime.Now;
                categoryName = amendment.CategoryName;
            }

            await _context.SaveChangesAsync();
            await UpdateUserActivityLogAsync(categoryName, ActivityType.Submitted);
            TempData["SuccessMessage"] = "Drafts submitted for review.";
            return RedirectToPage(new
            {
                SelectedDepartmentID,
                SelectedBudgetAmendmentMainID,
                SelectedStatusTab,
                SelectedFinancialYear,
                CustomStartDate = CustomStartDate?.ToString("yyyy-MM-dd"),
                CustomEndDate = CustomEndDate?.ToString("yyyy-MM-dd")
            });
        }

        public async Task<IActionResult> OnPostUpdateAmendmentStatusAsync(Guid transactionId, AmendmentStatus newStatus)
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            if (!_authService.IsAdminRole(HttpContext))
                return Forbid();

            // Retrieve all amendments with the specified TransactionId
            var budgetAmendments = await _context.BudgetAmendments
                .Where(ba => ba.TransactionId == transactionId)
                .ToListAsync();

            // Check if two related amendments exist
            if (budgetAmendments.Count != 2)
            {
                return NotFound();
            }

            // Update the status of both amendments
            foreach (var amendment in budgetAmendments)
            {
                amendment.Status = newStatus;

                amendment.UpdatedAt = DateTime.Now;
                amendment.UpdatedBy = _authService.GetAuthenticatedUserID(HttpContext);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage(new
            {
                SelectedDepartmentID,
                SelectedBudgetAmendmentMainID,
                SelectedStatusTab,
                SelectedFinancialYear,
                CustomStartDate = CustomStartDate?.ToString("yyyy-MM-dd"),
                CustomEndDate = CustomEndDate?.ToString("yyyy-MM-dd")
            });
        }

        public async Task<IActionResult> OnPostExportToExcelBudgetAmendmentsAsync(string exportType = "current")
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            var departments = await _context.Departments
                .ToDictionaryAsync(d => d.DepartmentID, d => d.DepartmentName);

            var query = _context.BudgetAmendments
                .Include(ba => ba.BudgetAmendmentMain)
                .Include(ba => ba.SpeedType)
                .Include(ba => ba.CreatedByUser)
                .AsQueryable();

            if (exportType == "current")
            {
                SetDefaultFinancialYearRange();
                var (startDate, endDate) = FinancialYearHelper.GetDateRange(SelectedFinancialYear, CustomStartDate, CustomEndDate);
                query = query.Where(ba => ba.BudgetAmendmentMain.ExtendedDeadline >= startDate && ba.BudgetAmendmentMain.ExtendedDeadline <= endDate);

                if (!string.IsNullOrEmpty(SelectedStatusTab) && Enum.TryParse<AmendmentStatus>(SelectedStatusTab, out var parsedStatus))
                    query = query.Where(b => b.Status == parsedStatus);

                if (SelectedDepartmentID != 0)
                    query = query.Where(b => b.DepartmentID == SelectedDepartmentID);

                if (SelectedBudgetAmendmentMainID.HasValue)
                    query = query.Where(b => b.BudgetAmendmentMainID == SelectedBudgetAmendmentMainID.Value);

                if (!string.IsNullOrEmpty(BudgetAmendmentSearchTerm))
                    query = query.Where(s => s.CategoryName.Contains(BudgetAmendmentSearchTerm)
                        || s.AdjustmentDetail.Contains(BudgetAmendmentSearchTerm)
                        || s.SpeedType.Code.Contains(BudgetAmendmentSearchTerm));
            }

            var amendments = await query.ToListAsync();

            using var workbook = new XLWorkbook();
            var date = DateTime.Now.ToString("yyyy-MM-dd");

            if (exportType == "summary")
            {
                var worksheet = workbook.Worksheets.Add("Summary");

                worksheet.Cell(1, 1).Value = "#";
                worksheet.Cell(1, 2).Value = "Amendment Name";
                worksheet.Cell(1, 3).Value = "Period";
                worksheet.Cell(1, 4).Value = "Department";
                worksheet.Cell(1, 5).Value = "Total Increase";
                worksheet.Cell(1, 6).Value = "Total Decrease";
                worksheet.Cell(1, 7).Value = "Net Change";
                worksheet.Cell(1, 8).Value = "Status";

                var headerRange = worksheet.Range(1, 1, 1, 8);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.Black;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#DCE6F1");
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.SheetView.FreezeRows(1);

                var groups = amendments
                    .GroupBy(ba => new { ba.BudgetAmendmentMainID, ba.DepartmentID })
                    .OrderBy(g => g.Key.BudgetAmendmentMainID)
                    .ThenBy(g => g.Key.DepartmentID);

                int row = 2;
                int serialNo = 1;
                foreach (var group in groups)
                {
                    var first = group.First();
                    var deptName = departments.TryGetValue(group.Key.DepartmentID, out var dn) ? dn : group.Key.DepartmentID.ToString();
                    var period = $"{first.BudgetAmendmentMain.StartDate:MMM d, yyyy} – {first.BudgetAmendmentMain.ExtendedDeadline:MMM d, yyyy}";
                    var totalIncrease = group.Sum(ba => ba.AmountIncrease);
                    var totalDecrease = group.Sum(ba => ba.AmountDecrease);
                    var overallStatus = group.Any(ba => ba.Status == AmendmentStatus.Submitted) ? "Submitted"
                        : group.All(ba => ba.Status == AmendmentStatus.Approved) ? "Approved"
                        : group.Any(ba => ba.Status == AmendmentStatus.Rejected) ? "Needs Review"
                        : "Unsubmitted Drafts";

                    worksheet.Cell(row, 1).Value = serialNo++;
                    worksheet.Cell(row, 2).Value = first.BudgetAmendmentMain.Name;
                    worksheet.Cell(row, 3).Value = period;
                    worksheet.Cell(row, 4).Value = deptName;
                    worksheet.Cell(row, 5).Value = totalIncrease;
                    worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
                    worksheet.Cell(row, 6).Value = totalDecrease;
                    worksheet.Cell(row, 6).Style.NumberFormat.Format = "$#,##0.00";
                    worksheet.Cell(row, 7).Value = totalIncrease - totalDecrease;
                    worksheet.Cell(row, 7).Style.NumberFormat.Format = "$#,##0.00";
                    worksheet.Cell(row, 8).Value = overallStatus;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BudgetAmendments_Summary_{date}.xlsx");
            }
            else
            {
                var worksheet = workbook.Worksheets.Add("Budget Amendments");

                worksheet.Cell(1, 1).Value = "#";
                worksheet.Cell(1, 2).Value = "Amendment Name";
                worksheet.Cell(1, 3).Value = "Period";
                worksheet.Cell(1, 4).Value = "Department";
                worksheet.Cell(1, 5).Value = "Category";
                worksheet.Cell(1, 6).Value = "SpeedType Code";
                worksheet.Cell(1, 7).Value = "Adjustment Detail";
                worksheet.Cell(1, 8).Value = "Amount Increase";
                worksheet.Cell(1, 9).Value = "Amount Decrease";
                worksheet.Cell(1, 10).Value = "Net Change";
                worksheet.Cell(1, 11).Value = "Status";
                worksheet.Cell(1, 12).Value = "Created At";
                worksheet.Cell(1, 13).Value = "Created By";

                var headerRange = worksheet.Range(1, 1, 1, 13);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.Black;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#DCE6F1");
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.SheetView.FreezeRows(1);

                int row = 2;
                int serialNo = 1;
                foreach (var amendment in amendments)
                {
                    var deptName = departments.TryGetValue(amendment.DepartmentID, out var dn) ? dn : amendment.DepartmentID.ToString();
                    var period = $"{amendment.BudgetAmendmentMain.StartDate:MMM d, yyyy} – {amendment.BudgetAmendmentMain.ExtendedDeadline:MMM d, yyyy}";

                    worksheet.Cell(row, 1).Value = serialNo++;
                    worksheet.Cell(row, 2).Value = amendment.BudgetAmendmentMain.Name;
                    worksheet.Cell(row, 3).Value = period;
                    worksheet.Cell(row, 4).Value = deptName;
                    worksheet.Cell(row, 5).Value = amendment.CategoryName;
                    worksheet.Cell(row, 6).Value = amendment.SpeedType?.Code;
                    worksheet.Cell(row, 7).Value = amendment.AdjustmentDetail;
                    worksheet.Cell(row, 8).Value = amendment.AmountIncrease;
                    worksheet.Cell(row, 8).Style.NumberFormat.Format = "$#,##0.00";
                    worksheet.Cell(row, 9).Value = amendment.AmountDecrease;
                    worksheet.Cell(row, 9).Style.NumberFormat.Format = "$#,##0.00";
                    worksheet.Cell(row, 10).Value = amendment.AmountIncrease - amendment.AmountDecrease;
                    worksheet.Cell(row, 10).Style.NumberFormat.Format = "$#,##0.00";
                    worksheet.Cell(row, 11).Value = amendment.Status.ToString();
                    worksheet.Cell(row, 12).Value = amendment.CreatedAt?.ToString("MMM d, yyyy");
                    worksheet.Cell(row, 13).Value = amendment.CreatedByUser?.Email;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                var label = exportType == "detail" ? "Detail" : "CurrentView";
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BudgetAmendments_{label}_{date}.xlsx");
            }
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

        private static bool AllCategoryNetChangesAreZero(List<BudgetAmendment> amendments) =>
            amendments
                .GroupBy(a => a.CategoryName)
                .All(g => g.Sum(a => (decimal)a.AmountIncrease) - g.Sum(a => (decimal)a.AmountDecrease) == 0);

        #endregion
    }
}