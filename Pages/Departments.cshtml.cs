using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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
        public string ActiveSortTable { get; set; } = "Employee";

        public string SortColumn { get; set; } = "EmployeeID";
        public string SortOrder { get; set; } = "asc";
        public List<int> PageSizes { get; set; } = new List<int> { 10, 20, 30 };
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

        public int DepartmentEmployees { get; set; }

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

            // ==============================================
            //                DEPARTMENT DATA
            // ==============================================
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

            Debug.WriteLine("=========HERE2===========");
            Debug.WriteLine(DepartmentResultsPerPage);
            Debug.WriteLine(DepartmentCurrentPage);

            var speedTypeQuery = _context.SpeedTypes.AsQueryable();
            SpeedTypes = await speedTypeQuery.ToListAsync();

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
        #endregion

        // ==============================================
        //              DEPARTMENT METHODS
        // ==============================================
        #region DEPARTMENT METHODS
        public async Task<IActionResult> OnPostAddDepartmentAsync()
        {
            if (!ModelState.IsValid)
            {

                /*                Debug.WriteLine("======== INVALID ===========");

                                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                                {
                                    Debug.WriteLine(error.ErrorMessage);
                                }


                                Debug.WriteLine("========  ===========");

                                Employees = await _context.Employees.ToListAsync(); // Re-fetch employees to display on the page
                                Departments = await _context.Departments.ToListAsync();  // Re-fetch departments to display on the page
                                SpeedTypes = await _context.SpeedTypes.ToListAsync();
                                BudgetAmendments = await _context.BudgetAmendments.ToListAsync();
                                return Page();*/
            }

            _context.Departments.Add(NewDepartment);
            await _context.SaveChangesAsync();

            foreach (var speedTypeId in SelectedSpeedTypeIds)
            {
                var departmentSpeedType = new DepartmentSpeedType
                {
                    DepartmentId = NewDepartment.DepartmentID,
                    SpeedTypeId = speedTypeId
                };

                _context.DepartmentSpeedTypes.Add(departmentSpeedType);
            }

            await _context.SaveChangesAsync();
            await LoadFormDataAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditDepartmentAsync(int id)
        {
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
            EditingDepartmentID = 0;
            await LoadFormDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveDepartmentAsync()
        {
            if (!ModelState.IsValid)
            {
                /*                Employees = await _context.Employees.ToListAsync();
                                Departments = await _context.Departments.ToListAsync();
                                return Page();*/
            }

            var department = await _context.Departments
                .Include(d => d.DepartmentSpeedTypes)
                .FirstOrDefaultAsync(d => d.DepartmentID == NewDepartment.DepartmentID);

            if (department != null)
            {
                department.DepartmentID = NewDepartment.DepartmentID;
                department.DepartmentName = NewDepartment.DepartmentName;
                department.DepartmentSpeedTypes.Clear();

                foreach (var speedTypeId in SelectedSpeedTypeIds)
                {
                    var existingSpeedType = department.DepartmentSpeedTypes
                        .Any(dst => dst.SpeedTypeId == speedTypeId);

                    if (!existingSpeedType)
                    {
                        department.DepartmentSpeedTypes.Add(new DepartmentSpeedType
                        {
                            DepartmentId = department.DepartmentID,
                            SpeedTypeId = speedTypeId
                        });
                    }
                    else
                    {
                        // TODO Optionally log or handle the case where the SpeedType already exists
                        // e.g., Debug.WriteLine($"SpeedTypeId {speedTypeId} already exists for department.");
                    }
                }

                Debug.WriteLine("========HERE in ONPOSTSAVFE || NOT NULL=========");

                Debug.WriteLine(department);
                Debug.WriteLine(department.DepartmentID);
                Debug.WriteLine(department.DepartmentName);
                Debug.WriteLine(department.DepartmentSpeedTypes.Count);
                Debug.WriteLine(SelectedSpeedTypeIds.Count);

                await _context.SaveChangesAsync();
            } else
            {
                Debug.WriteLine("========HERE in ONPOSTSAVFE || NULL=========");
            }

                await LoadFormDataAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteDepartmentAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                return NotFound();
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            await LoadFormDataAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExportToExcelDepartmentsAsync()
        {
            var departments = await _context.Departments
                .Include(d => d.DepartmentSpeedTypes)
                .ThenInclude(dst => dst.SpeedType)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Departments");

            worksheet.Cell(1, 1).Value = "Department ID";
            worksheet.Cell(1, 2).Value = "Department Name";
            worksheet.Cell(1, 3).Value = "SpeedType";
            worksheet.Cell(1, 4).Value = "Budget";

            int row = 2;
            foreach (var dept in departments)
            {
                foreach (var departmentSpeedType in dept.DepartmentSpeedTypes)
                {
                    worksheet.Cell(row, 1).Value = dept.DepartmentID;
                    worksheet.Cell(row, 2).Value = dept.DepartmentName;
                    worksheet.Cell(row, 3).Value = departmentSpeedType.SpeedType?.Code; // Assuming SpeedType has Code
                    worksheet.Cell(row, 4).Value = departmentSpeedType.SpeedType?.Budget; // Assuming SpeedType has Budget
                    row++;
                }
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Departments.xlsx");
        }
        #endregion
    }
}
