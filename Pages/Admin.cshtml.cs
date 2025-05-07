using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.Services;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class AdminModel : PageModel
    {
        // ==============================================
        // Instance Variables -- HELPER
        // ==============================================
        #region HELPER
        private readonly CasdbtestContext _context;
        private readonly IAuthenticationService _authService;

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
            public List<User> Users { get; set; }

            [BindProperty]
                public User NewUser { get; set; }

            [BindProperty]
                public int? EditingUserID { get; set; }

            [BindProperty(SupportsGet = true)]
            public string UserSearchTerm { get; set; }

            public int UserCurrentPage { get; set; } = 1;
            public int UserResultsPerPage { get; set; } = 10;
            public int UserTotalPages { get; set; }

            public int TotalUsers { get; set; }

            public List<SelectListItem> UserStatusOptions { get; set; }
            public List<SelectListItem> UserRoleOptions { get; set; }
        #endregion


        // ==============================================
        // Instance Variables -- DEPARTMENTS
        // ==============================================
        #region DEPARTMENTS
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

            public int SpeedTypeEmployees { get; set; }
            public List<int> SelectedSpeedTypeIds { get; set; } = new List<int>();

            [BindProperty(SupportsGet = true)]
            public decimal? SpeedTypeMinBudget { get; set; }

            [BindProperty(SupportsGet = true)]
            public decimal? SpeedTypeMaxBudget { get; set; }

        #endregion


        // ==============================================
        // Instance Variables -- BUDGETAMENDMENTSETTINGS
        // ==============================================
        #region BUDGETAMENDMENTSETTINGS
        [BindProperty]
            public BudgetAmendmentSetting BudgetAmendmentSetting { get; set; }

            public DateTime BudgetAmendmentStartDate { get; set; }

            public DateTime BudgetAmendmentEndDate { get; set; }
        #endregion


        // ==============================================
        //                 DATA LOADING
        // ==============================================
        #region DATA LOADING
            public AdminModel(CasdbtestContext context, IAuthenticationService authService)
            {
                _context = context;
                _authService = authService;
            }

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

                // ==============================================
                //                 SPEEDTYPE DATA
                // ==============================================
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

                // ==============================================
                //         BUDGET AMENDMENT SETTINGS DATA
                // ==============================================
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

                DepartmentCurrentPage = departmentPageNumber;
                DepartmentResultsPerPage = departmentResultsPerPage;

                SpeedTypeCurrentPage = speedTypePageNumber;
                SpeedTypeResultsPerPage = speedTypeResultsPerPage;

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

                    await _context.SaveChangesAsync();
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


        // ==============================================
        //              SPEEDTYPE METHODS
        // ==============================================
        #region SPEEDTYPE METHODS
            public async Task<IActionResult> OnPostAddSpeedTypeAsync()
            {
                if (!ModelState.IsValid)
                {
                    /*                Employees = await _context.Employees.ToListAsync(); // Re-fetch employees to display on the page
                                    Departments = await _context.Departments.ToListAsync();
                                    SpeedTypes = await _context.SpeedTypes.ToListAsync();  // Re-fetch speedtypes to display on the page
                                    BudgetAmendments = await _context.BudgetAmendments.ToListAsync();
                                    return Page();*/
                }

                _context.SpeedTypes.Add(NewSpeedType);
                await _context.SaveChangesAsync();
                await LoadFormDataAsync();
                return RedirectToPage();
            }

            public async Task<IActionResult> OnPostEditSpeedTypeAsync(int id)
            {
                EditingSpeedTypeID = id;
                await LoadFormDataAsync();
                return Page();
            }

            public async Task<IActionResult> OnPostCancelEditSpeedTypeAsync(int id)
            {
                EditingSpeedTypeID = 0;
                await LoadFormDataAsync();
                return Page();
            }

            public async Task<IActionResult> OnPostSaveSpeedTypeAsync()
            {
                if (!ModelState.IsValid)
                {
                    /* Employees = await _context.Employees.ToListAsync();
                       SpeedTypes = await _context.SpeedTypes.ToListAsync();
                       return Page(); */
                }

                var speedType = await _context.SpeedTypes.FindAsync(NewSpeedType.SpeedTypeId);

                if (speedType != null)
                {
                    speedType.SpeedTypeId = NewSpeedType.SpeedTypeId;
                    speedType.Code = NewSpeedType.Code;
                    speedType.Budget = NewSpeedType.Budget;

                    await _context.SaveChangesAsync();
                }

                await LoadFormDataAsync();
                return RedirectToPage();
            }

            public async Task<IActionResult> OnPostDeleteSpeedTypeAsync(int id)
            {
                var speedType = await _context.SpeedTypes.FindAsync(id);

                if (speedType == null)
                {
                    return NotFound();
                }

                _context.SpeedTypes.Remove(speedType);
                await _context.SaveChangesAsync();
                await LoadFormDataAsync();
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

        // ==============================================
        //       BUDGET AMENDMENT SETTINGS METHODS
        // ==============================================
        #region BUDGET AMENDMENT SETTINGS METHODS
        public async Task<IActionResult> OnPostSaveBudgetAmendmentDatesAsync()
        {
            if (!ModelState.IsValid)
            {
/*                return Page();*/
            }

            var existingSetting = await _context.BudgetAmendmentSettings.FirstOrDefaultAsync();

            if (existingSetting != null)
            {
                existingSetting.StartDate = BudgetAmendmentSetting.StartDate;
                existingSetting.EndDate = BudgetAmendmentSetting.EndDate;
                existingSetting.UpdatedAt = DateTime.Now;
            }
            else
            {
                BudgetAmendmentSetting.CreatedAt = DateTime.Now;
                BudgetAmendmentSetting.UpdatedAt = DateTime.Now;
                _context.BudgetAmendmentSettings.Add(BudgetAmendmentSetting);
            }

            await _context.SaveChangesAsync();
            await LoadFormDataAsync();
            return RedirectToPage();
        }
        #endregion
    }
}
