using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.ViewModels;
using budget_management_system_aspdotnetcore.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class SpeedTypesModel(CasdbtestContext context, IAuthenticationService authService) : PageModel
    {

        // ==============================================
        // Instance Variables -- HELPER
        // ==============================================
        #region HELPER
        private readonly CasdbtestContext _context = context;
        private readonly IAuthenticationService _authService = authService;

        public string userRole { get; set; } = "";
        public string ActiveSortTable { get; set; } = "SpeedType";

        public string SortColumn { get; set; } = "Code";
        public string SortOrder { get; set; } = "asc";
        public List<int> PageSizes { get; set; } = PaginationViewModel.DefaultPageSizes;
        #endregion

        // ==============================================
        // Instance Variables -- SPEEDTYPES
        // ==============================================
        #region SPEEDTYPES
        [BindProperty]
        public IEnumerable<SpeedType> SpeedTypes { get; set; }

        [BindProperty]
        public SpeedType NewSpeedType { get; set; }

        [BindProperty]
        public int? EditingSpeedTypeID { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SpeedTypeSearchTerm { get; set; }

        public int SpeedTypeCurrentPage { get; set; } = 1;
        public int SpeedTypeResultsPerPage { get; set; } = 10;
        public int SpeedTypeTotalPages { get; set; }

        public int TotalSpeedTypes { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? SpeedTypeMinBudget { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? SpeedTypeMaxBudget { get; set; }
        #endregion


        // ==============================================
        //                 DATA LOADING
        // ==============================================
        #region DATA LOADING
        public async Task LoadFormDataAsync()
        {
            userRole = _authService.GetUserRole(HttpContext);

            var speedTypeQuery = _context.SpeedTypes.AsQueryable();

            if (!string.IsNullOrEmpty(SpeedTypeSearchTerm))
            {
                speedTypeQuery = speedTypeQuery.Where(s => s.Code.Contains(SpeedTypeSearchTerm)
                || s.Budget.ToString().Contains(SpeedTypeSearchTerm));
            }

            if (!string.IsNullOrEmpty(SortColumn) && ActiveSortTable == "SpeedType")
            {
                speedTypeQuery = SortOrder == "asc"
                    ? speedTypeQuery.OrderBy(e => EF.Property<object>(e, SortColumn))
                    : speedTypeQuery.OrderByDescending(e => EF.Property<object>(e, SortColumn));
            }

            if (SpeedTypeMinBudget.HasValue)
            {
                speedTypeQuery = speedTypeQuery.Where(s => s.Budget >= SpeedTypeMinBudget.Value);
            }

            if (SpeedTypeMaxBudget.HasValue)
            {
                speedTypeQuery = speedTypeQuery.Where(s => s.Budget <= SpeedTypeMaxBudget.Value);
            }

            TotalSpeedTypes = await speedTypeQuery.CountAsync();
            SpeedTypeTotalPages = (int)Math.Ceiling(TotalSpeedTypes / (double)SpeedTypeResultsPerPage);

            SpeedTypes = await speedTypeQuery
                .Skip((SpeedTypeCurrentPage - 1) * SpeedTypeResultsPerPage)
                .Take(SpeedTypeResultsPerPage)
                .ToListAsync();
        }

        public async Task<IActionResult> OnGetAsync(
            int speedTypePageNumber = 1,
            int speedTypesPerPage = 10)
        {
            SpeedTypeCurrentPage = speedTypePageNumber;
            SpeedTypeResultsPerPage = speedTypesPerPage;

            if (!_authService.IsAuthenticated(HttpContext))
            {
                return RedirectToPage("/Login");
            }

            await LoadFormDataAsync();
            return Page();
        }

        public async Task OnGetSortColumn(string table, string column, string order,
            int speedTypePageNumber = 1, int speedTypesPerPage = 10)
        {
            SpeedTypeCurrentPage = speedTypePageNumber;
            SpeedTypeResultsPerPage = speedTypesPerPage;
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
        #endregion

        // ==============================================
        //              SPEEDTYPE METHODS
        // ==============================================
        #region SPEEDTYPE METHODS
        public async Task<IActionResult> OnPostAddSpeedTypeAsync()
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            _context.SpeedTypes.Add(NewSpeedType);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Speed type \"{NewSpeedType.Code}\" added successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditSpeedTypeAsync(int id)
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            EditingSpeedTypeID = id;
            await LoadFormDataAsync();
            NewSpeedType = SpeedTypes?.FirstOrDefault(s => s.SpeedTypeId == id)
                           ?? await _context.SpeedTypes.FindAsync(id);
            return Page();
        }

        public async Task<IActionResult> OnPostCancelEditSpeedTypeAsync(int id)
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            EditingSpeedTypeID = 0;
            await LoadFormDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveSpeedTypeAsync(int id)
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            var speedType = await _context.SpeedTypes.FindAsync(id);

            if (speedType != null)
            {
                speedType.Code = NewSpeedType.Code;
                speedType.Budget = NewSpeedType.Budget;
                speedType.FundCode = NewSpeedType.FundCode;
                speedType.ProgramCode = NewSpeedType.ProgramCode;
                speedType.ClassCode = NewSpeedType.ClassCode;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Speed type \"{speedType.Code}\" updated successfully.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteSpeedTypeAsync(int id)
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            var speedType = await _context.SpeedTypes.FindAsync(id);

            if (speedType == null)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = $"Speed type \"{speedType.Code}\" deleted.";
            _context.SpeedTypes.Remove(speedType);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExportToExcelSpeedTypesAsync()
        {
            var speedTypes = await _context.SpeedTypes
                .Include(st => st.DepartmentSpeedTypes)
                .ThenInclude(dst => dst.Department)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("SpeedTypes");

            worksheet.Cell(1, 1).Value = "SpeedType ID";
            worksheet.Cell(1, 2).Value = "Code";
            worksheet.Cell(1, 3).Value = "Budget";

            int row = 2;
            foreach (var speedType in speedTypes)
            {
                foreach (var departmentSpeedType in speedType.DepartmentSpeedTypes)
                {
                    worksheet.Cell(row, 1).Value = speedType.SpeedTypeId;
                    worksheet.Cell(row, 2).Value = speedType.Code;
                    worksheet.Cell(row, 3).Value = speedType.Budget;
                    row++;
                }
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SpeedTypes.xlsx");
        }
        #endregion
    }
}
