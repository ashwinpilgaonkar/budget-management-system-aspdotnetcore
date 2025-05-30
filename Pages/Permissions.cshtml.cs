using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class PermissionsModel(CasdbtestContext context, IAuthenticationService authService) : PageModel
    {
        // ==============================================
        // Instance Variables -- HELPER
        // ==============================================
        #region HELPER
        private readonly CasdbtestContext _context = context;
        private readonly IAuthenticationService _authService = authService;

        public bool isAdmin { get; set; } = false;
        public string ActiveSortTable { get; set; } = "Permission";

        public string SortColumn { get; set; } = "PermissionID";
        public string SortOrder { get; set; } = "asc";
        public List<int> PageSizes { get; set; } = new List<int> { 10, 20, 30 };
        #endregion

        // ==============================================
        // Instance Variables -- PERMISSIONS
        // ==============================================
        #region PERMISSIONS
        [BindProperty]
        public IEnumerable<Permission> Permissions { get; set; }

        [BindProperty]
        public Permission NewPermission { get; set; }

        [BindProperty]
        public int? EditingPermissionID { get; set; }

        [BindProperty(SupportsGet = true)]
        public string PermissionSearchTerm { get; set; }

        public int PermissionCurrentPage { get; set; } = 1;
        public int PermissionResultsPerPage { get; set; } = 10;
        public int PermissionTotalPages { get; set; }

        public int TotalPermissions { get; set; }
        #endregion

        // ==============================================
        // DATA LOADING
        // ==============================================
        #region DATA LOADING
        public async Task LoadFormDataAsync()
        {
            var permissionQuery = _context.Permissions.AsQueryable();

            if (!string.IsNullOrEmpty(PermissionSearchTerm))
            {
                permissionQuery = permissionQuery.Where(p => p.PermissionName.Contains(PermissionSearchTerm));
            }

            if (!string.IsNullOrEmpty(SortColumn) && ActiveSortTable == "Permission")
            {
                permissionQuery = SortOrder == "asc"
                    ? permissionQuery.OrderBy(e => EF.Property<object>(e, SortColumn))
                    : permissionQuery.OrderByDescending(e => EF.Property<object>(e, SortColumn));
            }

            TotalPermissions = await permissionQuery.CountAsync();
            PermissionTotalPages = (int)Math.Ceiling(TotalPermissions / (double)PermissionResultsPerPage);

            Permissions = await permissionQuery
                .Skip((PermissionCurrentPage - 1) * PermissionResultsPerPage)
                .Take(PermissionResultsPerPage)
                .ToListAsync();
        }

        public async Task<IActionResult> OnGetAsync(int permissionPageNumber = 1, int permissionResultsPerPage = 10)
        {
            PermissionCurrentPage = permissionPageNumber;
            PermissionResultsPerPage = permissionResultsPerPage;

            if (!_authService.IsAuthenticated(HttpContext))
            {
                return RedirectToPage("/Login");
            }

            if (!_authService.IsAdmin(HttpContext))
            {
                return RedirectToPage("/Index");
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
        // PERMISSION METHODS
        // ==============================================
        #region PERMISSION METHODS
        public async Task<IActionResult> OnPostAddPermissionAsync()
        {
/*            if (!ModelState.IsValid)
            {
                return Page();
            }*/

            _context.Permissions.Add(NewPermission);
            await _context.SaveChangesAsync();
            await LoadFormDataAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditPermissionAsync(int id)
        {
            EditingPermissionID = id;
            await LoadFormDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCancelEditPermissionAsync(int id)
        {
            EditingPermissionID = 0;
            await LoadFormDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSavePermissionAsync()
        {
/*            if (!ModelState.IsValid)
            {
                return Page();
            }*/

            var permission = await _context.Permissions.FindAsync(NewPermission.PermissionID);
            if (permission != null)
            {
                permission.PermissionName = NewPermission.PermissionName;
                // Add any additional fields here

                await _context.SaveChangesAsync();
            }

            await LoadFormDataAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeletePermissionAsync(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                return NotFound();
            }

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();
            await LoadFormDataAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExportToExcelPermissionsAsync()
        {
            var permissions = await _context.Permissions.ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Permissions");

            worksheet.Cell(1, 1).Value = "Permission ID";
            worksheet.Cell(1, 2).Value = "Name";

            int row = 2;
            foreach (var permission in permissions)
            {
                worksheet.Cell(row, 1).Value = permission.PermissionID;
                worksheet.Cell(row, 2).Value = permission.PermissionName;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Permissions.xlsx");
        }
        #endregion
    }
}
