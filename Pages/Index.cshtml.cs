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

namespace budget_management_system_aspdotnetcore.Pages
{
    public class IndexModel : PageModel
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

        public bool isAdmin { get; set; } = false;
        public string userRole{ get; set; } = "";

        public string ActiveSortTable { get; set; } = "Department";

        public string SortColumn { get; set; } = "DepartmentID";
        public string SortOrder { get; set; } = "asc";

        public List<int> PageSizes { get; set; } = new List<int> { 10, 20, 30 };

        public IEnumerable<SpeedType> SpeedTypes { get; set; }

        public List<BudgetAmendment> BudgetAmendments { get; set; }

        [BindProperty]
        public int? EditingBudgetAmendmentID { get; set; }

        [BindProperty]
        public BudgetAmendment NewBudgetAmendment { get; set; }

        public int BudgetAmendmentCurrentPage { get; set; } = 1;
        public int BudgetAmendmentResultsPerPage { get; set; } = 10;
        public int BudgetAmendmentTotalPages { get; set; }

        public int TotalBudgetAmendments { get; set; }

        [BindProperty(SupportsGet = true)]
        public string BudgetAmendmentSearchTerm { get; set; }

        [BindProperty]
        [Required]
        public double AmountTotal { get; set; }

        [BindProperty]
        [Required]
        public int SourceSpeedtype { get; set; }

        [BindProperty]
        [Required]
        public int DestinationSpeedtype { get; set; }

        public DateTime BudgetAmendmentStartDate { get; set; }
        public DateTime BudgetAmendmentEndDate { get; set; }

        [BindProperty]
        [Range(0.01, double.MaxValue, ErrorMessage = "Transfer amount must be greater than zero")]
        public double TransferAmount { get; set; }

        public List<Department> DepartmentsUserIsResponsibleFor { get; set; }
        [BindProperty(SupportsGet = true)]
        public int SelectedDepartmentID { get; set; } = 0;

        [BindProperty(SupportsGet = true)]
        public string SelectedStatusTab { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<string> SelectedStatus { get; set; }

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

        public int OverviewTotalCount { get; set; }
        public int OverviewApprovedCount { get; set; }
        public int OverviewPendingCount { get; set; }
        public int OverviewRejectedCount { get; set; }

        public int OverviewTotalActionsThisMonth { get; set; }

        public string OverviewMostActiveUser { get; set; }

        public DateTime OverviewLastActivityTime { get; set; }

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
        public void SetDefaultFinancialYearRange()
        {
            var today = DateTime.Today;
            int startYear = today.Month >= 10 ? today.Year : today.Year - 1;

            if(!CustomStartDate.HasValue)
                CustomStartDate = new DateTime(startYear, 10, 1);

            if(!CustomEndDate.HasValue)
                CustomEndDate = new DateTime(startYear + 1, 9, 30);
        }

        public void SetOverviewCardData()
        {
            DateTime startDate, endDate;

            if (string.IsNullOrEmpty(SelectedFinancialYear) && FinancialYearOptions.Any())
            {
                SelectedFinancialYear = FinancialYearOptions.First();
            }

            if (SelectedFinancialYear == "Custom" && CustomStartDate.HasValue && CustomEndDate.HasValue)
            {
                startDate = CustomStartDate.Value;
                endDate = CustomEndDate.Value;
            }
            else if (SelectedFinancialYear?.StartsWith("FY") == true)
            {
                var parts = SelectedFinancialYear.Substring(3).Split('-');
                startDate = new DateTime(int.Parse(parts[0]), 7, 1);
                endDate = new DateTime(int.Parse(parts[1]), 6, 30);
            }
            else
            {
                // Default fallback
                startDate = DateTime.MinValue;
                endDate = DateTime.Today.AddDays(1).AddTicks(-1);
            }

            var amendmentOverviewQuery = _context.BudgetAmendments
                .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate);

            var amendmentList = amendmentOverviewQuery.ToList();

            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var recentLogs = _context.UserActivityLogs
                .Include(log => log.User)
                .Where(log => log.Timestamp >= startOfMonth)
                .ToList();

            var mostActiveUser = recentLogs
                .GroupBy(log => log.User.Email)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            var lastActivityTime = recentLogs
                .OrderByDescending(log => log.Timestamp)
                .Select(log => log.Timestamp)
                .FirstOrDefault();

            OverviewTotalCount = amendmentList.Count;
            OverviewApprovedCount = amendmentList.Count(a => a.Status == AmendmentStatus.Approved);
            OverviewPendingCount = amendmentList.Count(a => a.Status == AmendmentStatus.Pending);
            OverviewRejectedCount = amendmentList.Count(a => a.Status == AmendmentStatus.Rejected);

            OverviewTotalActionsThisMonth = recentLogs.Count;
            OverviewMostActiveUser = mostActiveUser;
            OverviewLastActivityTime = lastActivityTime;
        }

        public IndexModel(CasdbtestContext context, SpeedTypeService speedTypeService, UserService userService, IAuthenticationService authService)
        {
            _context = context;
            _speedTypeService = speedTypeService;
            _userService = userService;
            _authService = authService;
        }

        public async Task LoadFormDataAsync()
        {
            userRole = _authService.GetUserRole(HttpContext);
            SetDefaultFinancialYearRange();
            SetOverviewCardData();


            if (SelectedStatus == null || !SelectedStatus.Any())
            {
                SelectedStatus = Enum.GetNames(typeof(AmendmentStatus)).ToList();
            }

            SpeedTypes = await _speedTypeService.GetSpeedTypesAsync();

            //Fetch BudgetAmendment Data
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

            if (SelectedStatus != null && SelectedStatus.Any())
            {
                var parsedStatuses = SelectedStatus
                    .Where(s => Enum.TryParse<AmendmentStatus>(s, out _))
                    .Select(s => (AmendmentStatus)Enum.Parse(typeof(AmendmentStatus), s));

                amendmentQuery = amendmentQuery.Where(b => parsedStatuses.Contains(b.Status));
            }

            if (!string.IsNullOrEmpty(SelectedStatusTab) && Enum.TryParse<AmendmentStatus>(SelectedStatusTab, out var parsedStatus))
            {
                amendmentQuery = amendmentQuery.Where(b => b.Status == parsedStatus);
            }

            if (SelectedDepartmentID != 0)
            {
                amendmentQuery = amendmentQuery.Where(b => b.DepartmentID == SelectedDepartmentID);
            }

            if (!string.IsNullOrEmpty(SortColumn) && ActiveSortTable == "AmendmentHistory")
            {
                amendmentQuery = SortOrder == "asc"
                    ? amendmentQuery.OrderBy(e => EF.Property<string>(e, SortColumn).ToString())
                    : amendmentQuery.OrderByDescending(e => EF.Property<string>(e, SortColumn).ToString());

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

            DateTime startDate, endDate;

            if (string.IsNullOrEmpty(SelectedFinancialYear) && FinancialYearOptions.Any())
            {
                SelectedFinancialYear = FinancialYearOptions.First();
            }

            if (SelectedFinancialYear == "Custom" && CustomStartDate.HasValue && CustomEndDate.HasValue)
            {
                startDate = CustomStartDate.Value;
                endDate = CustomEndDate.Value;
            }
            else if (SelectedFinancialYear?.StartsWith("FY") == true)
            {
                var parts = SelectedFinancialYear.Substring(3).Split('-');
                startDate = new DateTime(int.Parse(parts[0]), 7, 1);
                endDate = new DateTime(int.Parse(parts[1]), 6, 30);
            }
            else
            {
                // Default fallback
                startDate = DateTime.MinValue;
                endDate = DateTime.Today.AddDays(1).AddTicks(-1);
            }

            if (!CreatedFromDate.HasValue)
                CreatedFromDate = startDate;

            if (!CreatedToDate.HasValue)
                CreatedToDate = endDate;

            if (!EditedFromDate.HasValue)
                EditedFromDate = CreatedFromDate;

            if (!EditedToDate.HasValue)
                EditedToDate = DateTime.Today.AddDays(1).AddTicks(-1);

            if (!UpdatedFromDate.HasValue)
                UpdatedFromDate = CreatedFromDate;

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

            //Fetch Budget Amendment Settings
            var amendmentSettings = await _context.BudgetAmendmentSettings.FirstOrDefaultAsync();
            if (amendmentSettings != null)
            {
                BudgetAmendmentStartDate = amendmentSettings.StartDate;
                BudgetAmendmentEndDate = amendmentSettings.EndDate;
            }
            else
            {
                BudgetAmendmentStartDate = new DateTime(DateTime.Now.Year, 4, 1); // Example: April 1st as start
                BudgetAmendmentEndDate = BudgetAmendmentStartDate.AddYears(1).AddDays(-1); // March 31st as end
            }

            var userId = _authService.GetAuthenticatedUserID(HttpContext);
            DepartmentsUserIsResponsibleFor = _userService.GetDepartmentsResponsibleForLoggedInUser(userId).ToList();
        }


        public async Task<IActionResult> OnGetAsync(int amendmentPageNumber = 1, int amendmentResultsPerPage = 10)
        {
            BudgetAmendmentCurrentPage = amendmentPageNumber;
            BudgetAmendmentResultsPerPage = amendmentResultsPerPage;

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

        public string GetSortIcon(string column)
        {
            if (SortColumn == column)
            {
                return SortOrder == "asc" ? "fa-arrow-up" : "fa-arrow-down";
            }
            return "fa-sort";
        }

        public async Task<IActionResult> OnPostAddAmendmentAsync()
        {
            if (!ModelState.IsValid)
            {
                /*return Page();*/
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
                UpdatedAt = DateTime.Now
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
                UpdatedAt = DateTime.Now
            };

            _context.BudgetAmendments.Add(budgetAmendment1);
            _context.BudgetAmendments.Add(budgetAmendment2);
            await _context.SaveChangesAsync();

            await LoadFormDataAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAmendmentAsync(int id)
        {
            EditingBudgetAmendmentID = id;

            var amendment = await _context.BudgetAmendments.FindAsync(id);

            if (amendment != null)
            {
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



/*            if(amendment != null && amendment.AmountDecrease == 0)
            {
                SelectedSpeedType = amendment.SpeedTypeId;
            }*/

            await LoadFormDataAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCancelEditAmendmentAsync(int id)
        {
            EditingBudgetAmendmentID = 0;

            await LoadFormDataAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAmendmentAsync()
        {

            /*            if (!ModelState.IsValid)
                        {
                            await LoadFormDataAsync();
                            return Page(); // Return the same page to display validation errors
                        }*/

            var amendment = await _context.BudgetAmendments.FindAsync(NewBudgetAmendment.BudgetAmendmentID);

            if (amendment != null)
            {
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
                        destinationAmendment = relatedAmendments[0];
                        sourceAmendment = relatedAmendments[1];

                        sourceAmendment.SpeedTypeId = SourceSpeedtype;
                        destinationAmendment.SpeedTypeId = DestinationSpeedtype;
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
                        relatedAmendment.EditedBy = 1; // Set to current user's ID once available
                    }

                    sourceAmendment.AmountIncrease = NewBudgetAmendment.AmountDecrease;
                    sourceAmendment.AmountDecrease = NewBudgetAmendment.AmountIncrease;

                    destinationAmendment.AmountIncrease = NewBudgetAmendment.AmountIncrease;
                    destinationAmendment.AmountDecrease = NewBudgetAmendment.AmountDecrease;

                    await _context.SaveChangesAsync();
                }
                await UpdateUserActivityLogAsync(NewBudgetAmendment.CategoryName, ActivityType.Edited);
            }

            await LoadFormDataAsync();

            return RedirectToPage();
        }



        public async Task<IActionResult> OnPostDeleteAmendmentAsync(int id)
        {
            var amendment = await _context.BudgetAmendments.FindAsync(id);

            if (amendment == null)
            {
                return NotFound();
            }

            var categoryName = amendment.CategoryName;

            var transactionId = amendment.TransactionId;

            var relatedAmendments = _context.BudgetAmendments
                .Where(a => a.TransactionId == transactionId);

            _context.BudgetAmendments.RemoveRange(relatedAmendments);
            await _context.SaveChangesAsync();

            await UpdateUserActivityLogAsync(categoryName, ActivityType.Edited);

            await LoadFormDataAsync();

            return RedirectToPage();
        }

        /*        public async Task<IActionResult> OnPostApproveAmendmentAsync(int id)
                {
                    var budgetAmendment = await _context.BudgetAmendments.FindAsync(id);

                    if (budgetAmendment == null)
                    {
                        return NotFound();
                    }

                    budgetAmendment.UpdatedBy = 1;
                    budgetAmendment.UpdatedAt = DateTime.Now;

                    if(budgetAmendment.AmountIncrease != 0)
                    {
                        budgetAmendment.SpeedType.Budget = budgetAmendment.SpeedType.Budget + budgetAmendment.AmountIncrease;
                    } 
                    else
                    {

                    }

                    await OnPostUpdateAmendmentStatusAsync(budgetAmendment.TransactionId, AmendmentStatus.Approved);

                    return RedirectToPage();
                }*/

        /*        public async Task<IActionResult> OnPostApproveAmendmentAsync(int id)
                {
                    // Retrieve the initial budget amendment
                    var budgetAmendment = await _context.BudgetAmendments.FindAsync(id);

                    if (budgetAmendment == null)
                    {
                        return NotFound();
                    }

                    // Retrieve all amendments with the same TransactionId (should return a pair)
                    var amendmentPair = await _context.BudgetAmendments
                        .Where(a => a.TransactionId == budgetAmendment.TransactionId)
                        .Include(a => a.SpeedType) // Include SpeedType for accessing its Budget
                        .ToListAsync();

                    if (amendmentPair.Count != 2)
                    {
                        // Handle the case if there aren't exactly two amendments
                        return BadRequest("Amendment pair not found.");
                    }

                    // Determine source and destination amendments
                    var destinationAmendment = amendmentPair.FirstOrDefault(a => a.AmountIncrease != 0);
                    var sourceAmendment = amendmentPair.FirstOrDefault(a => a.AmountDecrease != 0);

                    if (destinationAmendment == null || sourceAmendment == null)
                    {
                        return BadRequest("Invalid amendment pair.");
                    }

                    // Update the Budget values
                    destinationAmendment.SpeedType.Budget += (decimal)destinationAmendment.AmountIncrease;
                    sourceAmendment.SpeedType.Budget -= (decimal)sourceAmendment.AmountDecrease;

                    // Update status and metadata for both amendments
                    foreach (var amendment in amendmentPair)
                    {
                        amendment.Status = AmendmentStatus.Approved;
                        amendment.UpdatedBy = 1;  // Set to current user's ID once available
                        amendment.UpdatedAt = DateTime.Now;
                    }

                    // Save all changes
                    await _context.SaveChangesAsync();

                    return RedirectToPage();
                }*/

        public async Task<IActionResult> OnPostApproveCategoryAsync(string category)
        {
            if (string.IsNullOrEmpty(category))
                return BadRequest("Invalid category");

            var amendments = _context.BudgetAmendments.Where(a => a.CategoryName == category);
            var categoryName = "";

            foreach (var amendment in amendments)
            {
                amendment.Status = AmendmentStatus.Approved;
                amendment.UpdatedBy = _authService.GetAuthenticatedUserID(HttpContext); 
                amendment.UpdatedAt = DateTime.Now;
                categoryName = amendment.CategoryName;
            }

            await _context.SaveChangesAsync();
            await UpdateUserActivityLogAsync(categoryName, ActivityType.Approved);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectCategoryAsync(string category)
        {
            if (string.IsNullOrEmpty(category))
                return BadRequest("Invalid category");

            var amendments = _context.BudgetAmendments.Where(a => a.CategoryName == category);
            var categoryName = "";

            foreach (var amendment in amendments)
            {
                amendment.Status = AmendmentStatus.Rejected;
                amendment.UpdatedBy = _authService.GetAuthenticatedUserID(HttpContext);
                amendment.UpdatedAt = DateTime.Now;
                categoryName = amendment.CategoryName;
            }

            await _context.SaveChangesAsync();
            await UpdateUserActivityLogAsync(categoryName, ActivityType.Rejected);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRevertCategoryAsync(string category)
        {
            if (string.IsNullOrEmpty(category))
                return BadRequest("Invalid category");

            var amendments = _context.BudgetAmendments.Where(a => a.CategoryName == category);
            var categoryName = "";

            foreach (var amendment in amendments)
            {
                amendment.Status = AmendmentStatus.Pending;
                amendment.UpdatedBy = _authService.GetAuthenticatedUserID(HttpContext);
                amendment.UpdatedAt = DateTime.Now;
                categoryName = amendment.CategoryName;
            }

            await _context.SaveChangesAsync();
            await UpdateUserActivityLogAsync(categoryName, ActivityType.Reverted);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostWithdrawCategoryAsync(string category)
        {
            if (string.IsNullOrEmpty(category))
                return BadRequest("Invalid category");

            var amendments = _context.BudgetAmendments.Where(a => a.CategoryName == category);
            var categoryName = "";

            foreach (var amendment in amendments)
            {
                amendment.Status = AmendmentStatus.Draft;
                amendment.UpdatedBy = _authService.GetAuthenticatedUserID(HttpContext);
                amendment.UpdatedAt = DateTime.Now;
                categoryName = amendment.CategoryName;
            }

            await _context.SaveChangesAsync();
            await UpdateUserActivityLogAsync(categoryName, ActivityType.Withdrawn);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSubmitCategoryAsync(string category)
        {
            if (string.IsNullOrEmpty(category))
                return BadRequest("Invalid category");

            var amendments = _context.BudgetAmendments.Where(a => a.CategoryName == category);
            var categoryName = "";

            foreach (var amendment in amendments)
            {
                amendment.Status = AmendmentStatus.Pending;
                amendment.UpdatedBy = _authService.GetAuthenticatedUserID(HttpContext);
                amendment.UpdatedAt = DateTime.Now;
                categoryName = amendment.CategoryName;
            }

            await _context.SaveChangesAsync();
            await UpdateUserActivityLogAsync(categoryName, ActivityType.Submitted);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSubmitSelectedCategories(string SelectedCategoryList)
        {
            var selectedCategories = SelectedCategoryList?.Split(',') ?? Array.Empty<string>();

            foreach (var category in selectedCategories)
            {
                var amendments = _context.BudgetAmendments
                                         .Where(a => a.CategoryName == category)
                                         .ToList();

                // Check if any amendment is not a Draft
                if (amendments.Any(a => a.Status != AmendmentStatus.Draft))
                {
                    TempData["Error"] = $"All amendments in category '{category}' must be Approved before submission.";
                    return RedirectToPage();
                }

                // If all are Drafts, set them to Pending
                foreach (var amendment in amendments)
                {
                    amendment.Status = AmendmentStatus.Pending;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Selected categories submitted successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExtendDeadlineAsync(string category, int extensionDays)
        {
            if (string.IsNullOrWhiteSpace(category) || extensionDays <= 0)
            {
                TempData["Error"] = "Invalid category or extension days.";
                return RedirectToPage(); // Stay on the same page
            }

            // Fetch matching amendments by category
            var amendments = await _context.BudgetAmendments
                .Where(a => a.CategoryName == category)
                .ToListAsync();

            if (amendments.Count == 0)
            {
                TempData["Error"] = "No amendments found for the selected category.";
                return RedirectToPage();
            }

            // Extend deadlines
            foreach (var amendment in amendments)
            { 
                amendment.ExtensionDays = extensionDays;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Deadline extended by {extensionDays} days for category '{category}'.";
            return RedirectToPage();
        }

        /*        public async Task<IActionResult> OnPostRejectAmendmentAsync(int id)
                {

                    var budgetAmendment = await _context.BudgetAmendments.FindAsync(id);

                    if (budgetAmendment == null)
                    {
                        return NotFound();
                    }

                    budgetAmendment.UpdatedBy = 1;
                    budgetAmendment.UpdatedAt = DateTime.Now;

                    await OnPostUpdateAmendmentStatusAsync(budgetAmendment.TransactionId, AmendmentStatus.Rejected);

                    return RedirectToPage();
                }*/

        public async Task<IActionResult> OnPostUpdateAmendmentStatusAsync(Guid transactionId, AmendmentStatus newStatus)
        {
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
            await LoadFormDataAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExportToExcelBudgetAmendmentsAsync()
        {
            var budgetAmendments = await _context.BudgetAmendments
                .Include(ba => ba.SpeedType) // Include related SpeedType information
                .Include(cb => cb.CreatedByUser)
                .ToListAsync();

            Debug.WriteLine(budgetAmendments[0].CreatedByUser?.Email);


            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Budget Amendments");

            // Define headers
            worksheet.Cell(1, 1).Value = "Budget Amendment ID";
            worksheet.Cell(1, 2).Value = "Category Name";
            worksheet.Cell(1, 3).Value = "Adjustment Detail";
            worksheet.Cell(1, 4).Value = "SpeedType ID";
            worksheet.Cell(1, 5).Value = "SpeedType Code";
            worksheet.Cell(1, 6).Value = "Fund Code";
            worksheet.Cell(1, 7).Value = "Department ID";
            worksheet.Cell(1, 8).Value = "Program Code";
            worksheet.Cell(1, 9).Value = "Class Code";
            worksheet.Cell(1, 10).Value = "Acct Description";
            worksheet.Cell(1, 11).Value = "Budget Code";
            worksheet.Cell(1, 12).Value = "Position Number";
            worksheet.Cell(1, 13).Value = "Amount Increase";
            worksheet.Cell(1, 14).Value = "Amount Decrease";
            worksheet.Cell(1, 15).Value = "Transaction ID";
            worksheet.Cell(1, 16).Value = "Status";
            worksheet.Cell(1, 17).Value = "Created At";
            worksheet.Cell(1, 18).Value = "Updated At";
            worksheet.Cell(1, 19).Value = "Edited At";
            worksheet.Cell(1, 20).Value = "Created By";
            worksheet.Cell(1, 21).Value = "Edited By";
            worksheet.Cell(1, 22).Value = "Updated By";

            // Populate data
            int row = 2;
            foreach (var amendment in budgetAmendments)
            {
                worksheet.Cell(row, 1).Value = amendment.BudgetAmendmentID;
                worksheet.Cell(row, 2).Value = amendment.CategoryName;
                worksheet.Cell(row, 3).Value = amendment.AdjustmentDetail;
                worksheet.Cell(row, 4).Value = amendment.SpeedTypeId;
                worksheet.Cell(row, 5).Value = amendment.SpeedType?.Code;
                worksheet.Cell(row, 6).Value = amendment.FundCode;
                worksheet.Cell(row, 7).Value = amendment.DepartmentID;
                worksheet.Cell(row, 8).Value = amendment.ProgramCode;
                worksheet.Cell(row, 9).Value = amendment.ClassCode;
                worksheet.Cell(row, 10).Value = amendment.AcctDescription;
                worksheet.Cell(row, 11).Value = amendment.BudgetCode;
                worksheet.Cell(row, 12).Value = amendment.PositionNumber;
                worksheet.Cell(row, 13).Value = amendment.AmountIncrease;
                worksheet.Cell(row, 14).Value = amendment.AmountDecrease;
                worksheet.Cell(row, 15).Value = amendment.TransactionId.ToString();
                worksheet.Cell(row, 16).Value = amendment.Status.ToString();
                worksheet.Cell(row, 17).Value = amendment.CreatedAt?.ToString("MM/dd/yyyy");
                worksheet.Cell(row, 18).Value = amendment.UpdatedAt?.ToString("MM/dd/yyyy");
                worksheet.Cell(row, 19).Value = amendment.EditedAt?.ToString("MM/dd/yyyy");
                worksheet.Cell(row, 20).Value = amendment.CreatedByUser?.Email;
                worksheet.Cell(row, 21).Value = amendment.EditedByUser?.Email;
                worksheet.Cell(row, 22).Value = amendment.UpdatedByUser?.Email;
                row++;
            }

            // Save the workbook to a memory stream
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BudgetAmendments.xlsx");
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