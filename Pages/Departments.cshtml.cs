using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.ViewModels;
using budget_management_system_aspdotnetcore.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class DepartmentsModel(CasdbtestContext context, IAuthenticationService authService) : PageModel
    {

        // ==============================================
        // Instance Variables -- HELPER
        // ==============================================
        #region HELPER
        private readonly CasdbtestContext _context = context;
        private readonly IAuthenticationService _authService = authService;

        public string userRole { get; set; } = "";
        public string ActiveSortTable { get; set; } = "Department";

        public string SortColumn { get; set; } = "DepartmentName";
        public string SortOrder { get; set; } = "asc";
        public List<int> PageSizes { get; set; } = PaginationViewModel.DefaultPageSizes;
        #endregion

        // ==============================================
        // Instance Variables -- DEPARTMENTS
        // ==============================================
        #region DEPARTMENTS

        [BindProperty]
        public List<int> SelectedSpeedTypeIds { get; set; } = new List<int>();

        [BindProperty]
        public IEnumerable<SpeedType> SpeedTypes { get; set; }

        public List<Department> Departments { get; set; }

        [BindProperty]
        public Department NewDepartment { get; set; }

        [BindProperty]
        public int? EditingDepartmentID { get; set; }

        [BindProperty(SupportsGet = true)]
        public string DepartmentSearchTerm { get; set; }

        public int DepartmentCurrentPage { get; set; } = 1;
        public int DepartmentResultsPerPage { get; set; } = 10;
        public int DepartmentTotalPages { get; set; }

        public int TotalDepartments { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? DepartmentMinBudget { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? DepartmentMaxBudget { get; set; }
        #endregion

        // ==============================================
        //                 DATA LOADING
        // ==============================================
        #region DATA LOADING

        public async Task LoadFormDataAsync()
        {
            userRole = _authService.GetUserRole(HttpContext);

            var departmentQuery = _context.Departments.AsQueryable();

            if (!string.IsNullOrEmpty(DepartmentSearchTerm))
            {
                departmentQuery = departmentQuery.Where(s => s.DepartmentName.Contains(DepartmentSearchTerm)
                    || s.DepartmentSpeedTypes.Any(ds => ds.SpeedType.Code.Contains(DepartmentSearchTerm)
                        || ds.SpeedType.Budget.ToString().Contains(DepartmentSearchTerm)));
            }

            if (!string.IsNullOrEmpty(SortColumn) && ActiveSortTable == "Department")
            {
                departmentQuery = SortOrder == "asc"
                    ? departmentQuery.OrderBy(e => EF.Property<object>(e, SortColumn))
                    : departmentQuery.OrderByDescending(e => EF.Property<object>(e, SortColumn));
            }

            if (DepartmentMinBudget.HasValue)
            {
                departmentQuery = departmentQuery
                    .Where(d => d.DepartmentSpeedTypes.Sum(ds => ds.SpeedType.Budget) >= DepartmentMinBudget.Value);
            }

            if (DepartmentMaxBudget.HasValue)
            {
                departmentQuery = departmentQuery
                    .Where(d => d.DepartmentSpeedTypes.Sum(ds => ds.SpeedType.Budget) <= DepartmentMaxBudget.Value);
            }

            TotalDepartments = await departmentQuery.CountAsync();
            DepartmentTotalPages = (int)Math.Ceiling(TotalDepartments / (double)DepartmentResultsPerPage);

            Departments = await departmentQuery
                .Include(d => d.DepartmentSpeedTypes)
                .ThenInclude(ds => ds.SpeedType)
                .Skip((DepartmentCurrentPage - 1) * DepartmentResultsPerPage)
                .Take(DepartmentResultsPerPage)
                .ToListAsync();

            SpeedTypes = await _context.SpeedTypes.ToListAsync();
        }

        public async Task<IActionResult> OnGetAsync(
            int departmentPageNumber = 1,
            int departmentResultsPerPage = 10)
        {
            DepartmentCurrentPage = departmentPageNumber;
            DepartmentResultsPerPage = departmentResultsPerPage;

            if (!_authService.IsAuthenticated(HttpContext))
            {
                return RedirectToPage("/Login");
            }

            await LoadFormDataAsync();
            return Page();
        }

        public async Task OnGetSortColumn(string table, string column, string order,
            int departmentPageNumber = 1, int departmentResultsPerPage = 10)
        {
            DepartmentCurrentPage = departmentPageNumber;
            DepartmentResultsPerPage = departmentResultsPerPage;
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
        //              DEPARTMENT METHODS
        // ==============================================
        #region DEPARTMENT METHODS
        public async Task<IActionResult> OnPostAddDepartmentAsync()
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            if (string.IsNullOrWhiteSpace(NewDepartment.DepartmentName) || !SelectedSpeedTypeIds.Any())
            {
                TempData["ErrorMessage"] = "Department Name and at least one SpeedType are required.";
                return RedirectToPage();
            }

            _context.Departments.Add(NewDepartment);
            await _context.SaveChangesAsync();

            foreach (var speedTypeId in SelectedSpeedTypeIds)
            {
                _context.DepartmentSpeedTypes.Add(new DepartmentSpeedType
                {
                    DepartmentId = NewDepartment.DepartmentID,
                    SpeedTypeId = speedTypeId
                });
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Department \"{NewDepartment.DepartmentName}\" added successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditDepartmentAsync(int id)
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            EditingDepartmentID = id;

            var department = await _context.Departments
                .Include(d => d.DepartmentSpeedTypes)
                .ThenInclude(ds => ds.SpeedType)
                .FirstOrDefaultAsync(d => d.DepartmentID == id);

            if (department == null)
            {
                return NotFound();
            }

            NewDepartment = department;
            SelectedSpeedTypeIds = department.DepartmentSpeedTypes.Select(ds => ds.SpeedTypeId).ToList();

            await LoadFormDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCancelEditDepartmentAsync(int id)
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            EditingDepartmentID = 0;
            await LoadFormDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveDepartmentAsync()
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            var department = await _context.Departments
                .Include(d => d.DepartmentSpeedTypes)
                .FirstOrDefaultAsync(d => d.DepartmentID == NewDepartment.DepartmentID);

            if (department != null)
            {
                department.DepartmentName = NewDepartment.DepartmentName;

                department.DepartmentSpeedTypes.Clear();
                foreach (var speedTypeId in SelectedSpeedTypeIds)
                {
                    department.DepartmentSpeedTypes.Add(new DepartmentSpeedType
                    {
                        DepartmentId = department.DepartmentID,
                        SpeedTypeId = speedTypeId
                    });
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Department \"{department.DepartmentName}\" updated successfully.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteDepartmentAsync(int id)
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = $"Department \"{department.DepartmentName}\" deleted.";
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExportToExcelDepartmentsAsync(bool exportAll = false)
        {
            var departmentQuery = _context.Departments
                .Include(d => d.DepartmentSpeedTypes)
                .ThenInclude(dst => dst.SpeedType)
                .AsQueryable();

            if (!exportAll)
            {
                if (!string.IsNullOrEmpty(DepartmentSearchTerm))
                {
                    departmentQuery = departmentQuery.Where(s => s.DepartmentName.Contains(DepartmentSearchTerm)
                        || s.DepartmentSpeedTypes.Any(ds => ds.SpeedType.Code.Contains(DepartmentSearchTerm)
                            || ds.SpeedType.Budget.ToString().Contains(DepartmentSearchTerm)));
                }
                if (DepartmentMinBudget.HasValue)
                {
                    departmentQuery = departmentQuery
                        .Where(d => d.DepartmentSpeedTypes.Sum(ds => ds.SpeedType.Budget) >= DepartmentMinBudget.Value);
                }
                if (DepartmentMaxBudget.HasValue)
                {
                    departmentQuery = departmentQuery
                        .Where(d => d.DepartmentSpeedTypes.Sum(ds => ds.SpeedType.Budget) <= DepartmentMaxBudget.Value);
                }
            }

            var departments = await departmentQuery.ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Departments");

            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Department Name";
            worksheet.Cell(1, 3).Value = "SpeedType";
            worksheet.Cell(1, 4).Value = "Budget";

            var headerRange = worksheet.Range(1, 1, 1, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontColor = XLColor.Black;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#DCE6F1");
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.SheetView.FreezeRows(1);

            int row = 2;
            int serialNo = 1;
            foreach (var dept in departments)
            {
                foreach (var departmentSpeedType in dept.DepartmentSpeedTypes)
                {
                    worksheet.Cell(row, 1).Value = serialNo++;
                    worksheet.Cell(row, 2).Value = dept.DepartmentName;
                    worksheet.Cell(row, 3).Value = departmentSpeedType.SpeedType?.Code;
                    worksheet.Cell(row, 4).Value = departmentSpeedType.SpeedType?.Budget;
                    worksheet.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
                    row++;
                }
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var date = DateTime.Now.ToString("yyyy-MM-dd");
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Departments_{date}.xlsx");
        }
        #endregion
    }
}
