using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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

        #endregion

        // ==============================================
        //                 DATA LOADING
        // ==============================================
        #region DATA LOADING

        public async Task LoadFormDataAsync()
        {
            isAdmin = _authService.IsAdmin(HttpContext);

            // ==============================================
            //                  USER DATA
            // ==============================================
            var usersQuery = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(UserSearchTerm))
            {
                usersQuery = usersQuery.Where(s => s.FirstName.Contains(UserSearchTerm)
                || s.LastName.ToString().Contains(UserSearchTerm)
                || s.Email.ToString().Contains(UserSearchTerm)
                || s.PhoneNumber.ToString().Contains(UserSearchTerm)
                || s.HireDate.ToString().Contains(UserSearchTerm)
                || s.JobTitle.ToString().Contains(UserSearchTerm)
                || s.Salary.ToString().Contains(UserSearchTerm)
                || s.Department.DepartmentName.ToString().Contains(UserSearchTerm));
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
                .Skip((UserCurrentPage - 1) * UserResultsPerPage)
                .Take(UserResultsPerPage)
                .ToListAsync();

            UserStatusOptions = Enum.GetValues(typeof(UserStatus))
                .Cast<UserStatus>()
                .Select(u => new SelectListItem { Value = u.ToString(), Text = u.ToString() })
                .ToList();

            UserRoleOptions = Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .Select(u => new SelectListItem { Value = u.ToString(), Text = u.ToString() })
                .ToList();


            // ==============================================
            //                DEPARTMENT DATA
            // ==============================================
            var departmentQuery = _context.Departments.AsQueryable();
            Departments = await departmentQuery.ToListAsync();
        }

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1,
            int resultsPerPage = 10,
            int departmentPageNumber = 1,
            int departmentResultsPerPage = 10,
            int speedTypePageNumber = 1,
            int speedTypeResultsPerPage = 10)
        {
            UserCurrentPage = pageNumber;
            UserResultsPerPage = resultsPerPage;

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
                user.Role = NewUser.Role;
                user.PhoneNumber = NewUser.PhoneNumber;
                user.HireDate = NewUser.HireDate;
                user.JobTitle = NewUser.JobTitle;
                user.Salary = NewUser.Salary;
                user.DepartmentID = NewUser.DepartmentID;

                await _context.SaveChangesAsync();
            }

            await LoadFormDataAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(NewUser.UserId);

            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(NewUser);
            await _context.SaveChangesAsync();
            await LoadFormDataAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExportToExcelUsersAsync()
        {
            var users = await _context.Users.Include(e => e.Department).ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Users");

            worksheet.Cell(1, 1).Value = "User ID";
            worksheet.Cell(1, 2).Value = "First Name";
            worksheet.Cell(1, 3).Value = "Last Name";
            worksheet.Cell(1, 5).Value = "Email";
            worksheet.Cell(1, 6).Value = "Phone Number";
            worksheet.Cell(1, 7).Value = "Hire Date";
            worksheet.Cell(1, 8).Value = "Job Title";
            worksheet.Cell(1, 9).Value = "Salary";
            worksheet.Cell(1, 10).Value = "Department";

            int row = 2;
            foreach (var user in users)
            {
                worksheet.Cell(row, 1).Value = user.UserId;
                worksheet.Cell(row, 2).Value = user.FirstName;
                worksheet.Cell(row, 3).Value = user.LastName;
                worksheet.Cell(row, 5).Value = user.Email;
                worksheet.Cell(row, 6).Value = user.PhoneNumber;
                worksheet.Cell(row, 7).Value = user.HireDate.ToString("MM/dd/yyyy");
                worksheet.Cell(row, 8).Value = user.JobTitle;
                worksheet.Cell(row, 9).Value = user.Salary;
                worksheet.Cell(row, 10).Value = user.Department?.DepartmentName;
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