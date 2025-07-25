using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

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
        public string ActiveSortTable { get; set; } = "Employee";

        public string SortColumn { get; set; } = "EmployeeID";
        public string SortOrder { get; set; } = "asc";
        public List<int> PageSizes { get; set; } = new List<int> { 10, 20, 30 };
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

            // ==============================================
            //                  USER DATA
            // ==============================================
            var usersQuery = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(UserSearchTerm))
            {
                usersQuery = usersQuery.Where(s => s.FirstName.Contains(UserSearchTerm)
                || s.LastName.ToString().Contains(UserSearchTerm)
                || s.Email.ToString().Contains(UserSearchTerm));
            }

            if (!string.IsNullOrEmpty(SortColumn) && ActiveSortTable == "User")
            {
                usersQuery = SortOrder == "asc"
                    ? usersQuery.OrderBy(e => EF.Property<object>(e, SortColumn))
                    : usersQuery.OrderByDescending(e => EF.Property<object>(e, SortColumn));
            }

            TotalUsers = await usersQuery.CountAsync();
            UserTotalPages = (int)Math.Ceiling(TotalUsers / (double)UserResultsPerPage);

            Users = await usersQuery
                .Include(u => u.DepartmentsResponsibleFor)
                .Include(u => u.Role)
                .Skip((UserCurrentPage - 1) * UserResultsPerPage)
                .Take(UserResultsPerPage)
                .ToListAsync();

            Debug.WriteLine("=======GERE=====");
            Debug.WriteLine(Users.Count);

            UserStatusOptions = Enum.GetValues(typeof(UserStatus))
                .Cast<UserStatus>()
                .Select(u => new SelectListItem { Value = u.ToString(), Text = u.ToString() })
                .ToList();


            // ==============================================
            //                ROLE DATA
            // ==============================================

            UserRoleOptions = await _context.Roles
            .Select(r => new SelectListItem
            {
                Value = r.RoleID.ToString(),
                Text = r.RoleName
            }).ToListAsync();

            // ==============================================
            //                DEPARTMENT DATA
            // ==============================================
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
        //               USER METHODS
        // ==============================================
        #region USER METHODS

        public async Task<IActionResult> OnPostAddUserAsync()
        {
            /*                if (!ModelState.IsValid)
                            {
            *//*                                    Employees = await _context.Employees.ToListAsync(); // Re-fetch employees to display on the page
                                                Departments = await _context.Departments.ToListAsync();  // Re-fetch departments to display on the page
                                                return Page();*//*
                            }

                            byte[] salt = new byte[16];
                            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
                            {
                                rng.GetBytes(salt);
                            }

                            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                                password: NewUser.Password,
                                salt: salt,
                                prf: KeyDerivationPrf.HMACSHA256,
                                iterationCount: 10000,
                                numBytesRequested: 32));

                            NewUser.Password = hashedPassword;
                            NewUser.Salt = Convert.ToBase64String(salt);

                            _context.Users.Add(NewUser);
                            await _context.SaveChangesAsync();

                            await LoadFormDataAsync();
                            return RedirectToPage();*/

            if (!ModelState.IsValid)
            {
                /*                    return Page();*/
            }

            // Hash password using BCrypt
            NewUser.Password = BCrypt.Net.BCrypt.HashPassword(NewUser.Password);

            NewUser.DepartmentsResponsibleFor = await _context.Departments
                .Where(d => SelectedDepartmentIds.Contains(d.DepartmentID))
                .ToListAsync();

            _context.Users.Add(NewUser);
            await _context.SaveChangesAsync();

            await LoadFormDataAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditUserAsync(int id)
        {
            EditingUserID = id;
            await LoadFormDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCancelEditUserAsync(int id)
        {
            EditingUserID = 0;
            await LoadFormDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveUserAsync()
        {

            if (!ModelState.IsValid)
            {
                /*                Employees = await _context.Employees.ToListAsync();
                                Departments = await _context.Departments.ToListAsync();
                                return Page();*/
            }

            var user = await _context.Users.FindAsync(NewUser.UserId);

            if (user != null)
            {
                user.Email = NewUser.Email;
                user.FirstName = NewUser.FirstName;
                user.LastName = NewUser.LastName;
                user.Status = NewUser.Status;
/*                user.Role = NewUser.Role;*/

                await _context.SaveChangesAsync();
            }

            await LoadFormDataAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            await LoadFormDataAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExportToExcelUsersAsync()
        {
            var users = await _context.Users.ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Users");

            worksheet.Cell(1, 1).Value = "User ID";
            worksheet.Cell(1, 5).Value = "Email";
            worksheet.Cell(1, 2).Value = "First Name";
            worksheet.Cell(1, 3).Value = "Last Name";
            worksheet.Cell(1, 5).Value = "Status";
            worksheet.Cell(1, 5).Value = "Role";
            worksheet.Cell(1, 10).Value = "Departments Responsible for";

            int row = 2;
            foreach (var user in users)
            {
                worksheet.Cell(row, 1).Value = user.UserId;
                worksheet.Cell(row, 2).Value = user.Email;
                worksheet.Cell(row, 3).Value = user.FirstName;
                worksheet.Cell(row, 4).Value = user.LastName;
                worksheet.Cell(row, 5).Value = user.Status.ToString();
/*                worksheet.Cell(row, 6).Value = user.Role.RoleName;*/
                worksheet.Cell(row, 7).Value = string.Join(", ", user.DepartmentsResponsibleFor.Select(d => d.DepartmentName));
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Employees.xlsx");
        }
        #endregion
    }
}