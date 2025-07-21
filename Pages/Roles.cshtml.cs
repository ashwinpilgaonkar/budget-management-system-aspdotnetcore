using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class RolesModel(CasdbtestContext context, IAuthenticationService authService) : PageModel
    {
        // ==============================================
        // Instance Variables -- HELPER
        // ==============================================
        #region HELPER
        private readonly CasdbtestContext _context = context;
        private readonly IAuthenticationService _authService = authService;

        public string userRole { get; set; } = "";
        public string ActiveSortTable { get; set; } = "Role";

        public string SortColumn { get; set; } = "RoleID";
        public string SortOrder { get; set; } = "asc";
        public List<int> PageSizes { get; set; } = new List<int> { 10, 20, 30 };
        #endregion

        // ==============================================
        // Instance Variables -- ROLES
        // ==============================================
        #region ROLES
        [BindProperty]
        public IEnumerable<Role> Roles { get; set; }

        [BindProperty]
        public Role NewRole { get; set; }

        [BindProperty]
        public int? EditingRoleID { get; set; }

        [BindProperty(SupportsGet = true)]
        public string RoleSearchTerm { get; set; }

        public int RoleCurrentPage { get; set; } = 1;
        public int RoleResultsPerPage { get; set; } = 10;
        public int RoleTotalPages { get; set; }

        public int TotalRoles { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedRoleID { get; set; }

        #endregion

        // ==============================================
        // DATA LOADING
        // ==============================================
        #region DATA LOADING
        public async Task LoadFormDataAsync()
        {
            userRole = _authService.GetUserRole(HttpContext);

            var roleQuery = _context.Roles.AsQueryable();

            if (!string.IsNullOrEmpty(RoleSearchTerm))
            {
                roleQuery = roleQuery.Where(r => r.RoleName.Contains(RoleSearchTerm));
            }

            if (!string.IsNullOrEmpty(SortColumn) && ActiveSortTable == "Role")
            {
                roleQuery = SortOrder == "asc"
                    ? roleQuery.OrderBy(e => EF.Property<object>(e, SortColumn))
                    : roleQuery.OrderByDescending(e => EF.Property<object>(e, SortColumn));
            }

            TotalRoles = await roleQuery.CountAsync();
            RoleTotalPages = (int)Math.Ceiling(TotalRoles / (double)RoleResultsPerPage);

            Roles = await roleQuery
                .Skip((RoleCurrentPage - 1) * RoleResultsPerPage)
                .Take(RoleResultsPerPage)
                .ToListAsync();
        }

        public async Task<IActionResult> OnGetAsync(int rolePageNumber = 1, int roleResultsPerPage = 10)
        {
            RoleCurrentPage = rolePageNumber;
            RoleResultsPerPage = roleResultsPerPage;

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

        public string SortOrderForColumn(string column) =>
            SortColumn == column && SortOrder == "asc" ? "desc" : "asc";

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
        // ROLE METHODS
        // ==============================================
        #region ROLE METHODS
        public async Task<IActionResult> OnPostAddRoleAsync()
        {
/*            if (!ModelState.IsValid)
            {
                return Page();
            }*/

            _context.Roles.Add(NewRole);
            await _context.SaveChangesAsync();
            await LoadFormDataAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditRoleAsync(int id)
        {
            EditingRoleID = id;
            await LoadFormDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCancelEditRoleAsync(int id)
        {
            EditingRoleID = 0;
            await LoadFormDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveRoleAsync()
        {
/*            if (!ModelState.IsValid)
            {
                return Page();
            }*/

            var role = await _context.Roles.FindAsync(NewRole.RoleID);

            Debug.WriteLine("======HERE========");
            Debug.WriteLine(role);
            Debug.WriteLine(NewRole);
            Debug.WriteLine(NewRole.RoleID);

            if (role != null)
            {
                Debug.WriteLine("======NULL========");

                role.RoleName = NewRole.RoleName;

                await _context.SaveChangesAsync();
            }

            await LoadFormDataAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteRoleAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            await LoadFormDataAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExportToExcelRolesAsync()
        {
            var roles = await _context.Roles.ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Roles");

            worksheet.Cell(1, 1).Value = "Role ID";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Description";

            int row = 2;
            foreach (var role in roles)
            {
                worksheet.Cell(row, 1).Value = role.RoleID;
                worksheet.Cell(row, 2).Value = role.RoleName;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Roles.xlsx");
        }

        #endregion
    }
}
