using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.ViewModels;
using budget_management_system_aspdotnetcore.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class UsersModel(CasdbtestContext context, IAuthenticationService authService) : PageModel
    {

        // ==============================================
        // Instance Variables -- HELPER
        // ==============================================
        #region HELPER
        private readonly CasdbtestContext _context = context;
        private readonly IAuthenticationService _authService = authService;

        public bool isAdmin { get; set; } = false;
        public string userRole { get; set; } = "";
        public string ActiveSortTable { get; set; } = "User";

        [BindProperty(SupportsGet = true)]
        public string SortColumn { get; set; } = "Email";
        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; } = "asc";
        public List<int> PageSizes { get; set; } = PaginationViewModel.DefaultPageSizes;
        #endregion

        // ==============================================
        // Instance Variables -- USERS
        // ==============================================
        #region USERS

        public List<Department>? Departments { get; set; }
        public List<User>? Users { get; set; }

        [BindProperty]
        public required User NewUser { get; set; }

        [BindProperty]
        public int? EditingUserID { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? UserSearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? UserStatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? UserRoleIdFilter { get; set; }

        public int UserCurrentPage { get; set; } = 1;
        public int UserResultsPerPage { get; set; } = 10;
        public int UserTotalPages { get; set; }

        public int TotalUsers { get; set; }

        public List<SelectListItem>? UserStatusOptions { get; set; }
        public List<SelectListItem>? UserRoleOptions { get; set; }

        [Required]
        [BindProperty]
        public List<int> SelectedDepartmentIds { get; set; } = new();
        public List<SelectListItem> DepartmentOptions { get; set; } = new();

        #endregion

        // ==============================================
        //                 DATA LOADING
        // ==============================================
        #region DATA LOADING

        public async Task LoadFormDataAsync()
        {
            userRole = _authService.GetUserRole(HttpContext);

            var usersQuery = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(UserSearchTerm))
            {
                usersQuery = usersQuery.Where(s => s.FirstName.Contains(UserSearchTerm)
                || s.LastName.Contains(UserSearchTerm)
                || s.Email.Contains(UserSearchTerm));
            }

            if (!string.IsNullOrEmpty(UserStatusFilter) && Enum.TryParse<UserStatus>(UserStatusFilter, out var statusFilter))
            {
                usersQuery = usersQuery.Where(u => u.Status == statusFilter);
            }

            if (UserRoleIdFilter.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.RoleID == UserRoleIdFilter.Value);
            }

            if (!string.IsNullOrEmpty(SortColumn) && ActiveSortTable == "User")
            {
                if (SortColumn == "Role")
                {
                    usersQuery = SortOrder == "asc"
                        ? usersQuery.OrderBy(u => u.Role.RoleName)
                        : usersQuery.OrderByDescending(u => u.Role.RoleName);
                }
                else
                {
                    usersQuery = SortOrder == "asc"
                        ? usersQuery.OrderBy(e => EF.Property<object>(e, SortColumn))
                        : usersQuery.OrderByDescending(e => EF.Property<object>(e, SortColumn));
                }
            }

            TotalUsers = await usersQuery.CountAsync();
            UserTotalPages = (int)Math.Ceiling(TotalUsers / (double)UserResultsPerPage);

            Users = await usersQuery
                .Include(u => u.DepartmentsResponsibleFor)
                .Include(u => u.Role)
                .Skip((UserCurrentPage - 1) * UserResultsPerPage)
                .Take(UserResultsPerPage)
                .ToListAsync();

            UserStatusOptions = Enum.GetValues(typeof(UserStatus))
                .Cast<UserStatus>()
                .Select(u => new SelectListItem { Value = u.ToString(), Text = u.ToString() })
                .ToList();

            UserRoleOptions = await _context.Roles
            .Select(r => new SelectListItem
            {
                Value = r.RoleID.ToString(),
                Text = r.RoleName
            }).ToListAsync();

            DepartmentOptions = await _context.Departments
                .OrderBy(d => d.DepartmentName)
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmentID.ToString(),
                    Text = d.DepartmentName
                })
                .ToListAsync();
        }

        public async Task<IActionResult> OnGetAsync(
            int userPageNumber = 1,
            int userResultsPerPage = 10)
        {
            UserCurrentPage = userPageNumber;
            UserResultsPerPage = userResultsPerPage;

            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            if (!_authService.IsAdminRole(HttpContext))
                return Forbid();

            await LoadFormDataAsync();
            return Page();
        }

        public async Task OnGetSortColumn(string table, string column, string order,
            int userPageNumber = 1, int userResultsPerPage = 10)
        {
            UserCurrentPage = userPageNumber;
            UserResultsPerPage = userResultsPerPage;
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
        //               USER METHODS
        // ==============================================
        #region USER METHODS

        public async Task<IActionResult> OnPostAddUserAsync()
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            if (string.IsNullOrWhiteSpace(NewUser.Email) || string.IsNullOrWhiteSpace(NewUser.Password)
                || string.IsNullOrWhiteSpace(NewUser.FirstName) || string.IsNullOrWhiteSpace(NewUser.LastName)
                || !SelectedDepartmentIds.Any())
            {
                TempData["ErrorMessage"] = "All user fields are required.";
                return RedirectToPage();
            }

            NewUser.Password = BCrypt.Net.BCrypt.HashPassword(NewUser.Password);

            NewUser.DepartmentsResponsibleFor = await _context.Departments
                .Where(d => SelectedDepartmentIds.Contains(d.DepartmentID))
                .ToListAsync();

            _context.Users.Add(NewUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"User {NewUser.FirstName} {NewUser.LastName} added successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditUserAsync(int id)
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            EditingUserID = id;

            // Pre-populate SelectedDepartmentIds so the edit row multi-select is pre-selected
            var user = await _context.Users
                .Include(u => u.DepartmentsResponsibleFor)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user != null)
            {
                SelectedDepartmentIds = user.DepartmentsResponsibleFor.Select(d => d.DepartmentID).ToList();
            }

            await LoadFormDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCancelEditUserAsync(int id)
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            EditingUserID = 0;
            await LoadFormDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveUserAsync()
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            var user = await _context.Users
                .Include(u => u.DepartmentsResponsibleFor)
                .FirstOrDefaultAsync(u => u.UserId == NewUser.UserId);

            if (user != null)
            {
                user.Email = NewUser.Email;
                user.FirstName = NewUser.FirstName;
                user.LastName = NewUser.LastName;
                user.Status = NewUser.Status;
                user.RoleID = NewUser.RoleID;

                user.DepartmentsResponsibleFor = await _context.Departments
                    .Where(d => SelectedDepartmentIds.Contains(d.DepartmentID))
                    .ToListAsync();

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"User {user.FirstName} {user.LastName} updated successfully.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(int id)
        {
            if (!_authService.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = $"User {user.FirstName} {user.LastName} deleted.";
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExportToExcelUsersAsync(bool exportAll = false)
        {
            var usersQuery = _context.Users
                .Include(u => u.Role)
                .Include(u => u.DepartmentsResponsibleFor)
                .AsQueryable();

            if (!exportAll)
            {
                if (!string.IsNullOrEmpty(UserSearchTerm))
                {
                    usersQuery = usersQuery.Where(s => s.FirstName.Contains(UserSearchTerm)
                        || s.LastName.Contains(UserSearchTerm)
                        || s.Email.Contains(UserSearchTerm));
                }

                if (!string.IsNullOrEmpty(UserStatusFilter) && Enum.TryParse<UserStatus>(UserStatusFilter, out var statusFilter))
                {
                    usersQuery = usersQuery.Where(u => u.Status == statusFilter);
                }

                if (UserRoleIdFilter.HasValue)
                {
                    usersQuery = usersQuery.Where(u => u.RoleID == UserRoleIdFilter.Value);
                }
            }

            var users = await usersQuery.ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Users");

            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Email";
            worksheet.Cell(1, 3).Value = "First Name";
            worksheet.Cell(1, 4).Value = "Last Name";
            worksheet.Cell(1, 5).Value = "Status";
            worksheet.Cell(1, 6).Value = "Role";
            worksheet.Cell(1, 7).Value = "Departments Responsible For";

            var headerRange = worksheet.Range(1, 1, 1, 7);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontColor = XLColor.Black;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#DCE6F1");
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.SheetView.FreezeRows(1);

            int row = 2;
            int serialNo = 1;
            foreach (var user in users)
            {
                worksheet.Cell(row, 1).Value = serialNo++;
                worksheet.Cell(row, 2).Value = user.Email;
                worksheet.Cell(row, 3).Value = user.FirstName;
                worksheet.Cell(row, 4).Value = user.LastName;
                worksheet.Cell(row, 5).Value = user.Status.ToString();
                worksheet.Cell(row, 6).Value = user.Role.RoleName;
                worksheet.Cell(row, 7).Value = string.Join(", ", user.DepartmentsResponsibleFor.Select(d => d.DepartmentName));
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var date = DateTime.Now.ToString("yyyy-MM-dd");
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Users_{date}.xlsx");
        }
        #endregion
    }
}
