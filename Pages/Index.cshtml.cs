using budget_management_system_aspdotnetcore.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class IndexModel : PageModel
    {
        private readonly CasdbtestContext _context;

        public IndexModel(CasdbtestContext context)
        {
            _context = context;
        }

        public List<Employee> Employees { get; set; }

        [BindProperty]
        public int? EditingEmployeeID { get; set; }

        [BindProperty]
        public Employee NewEmployee { get; set; }

        public int EmployeeCurrentPage { get; set; } = 1;
        public int EmployeeResultsPerPage { get; set; } = 10;
        public int EmployeeTotalPages { get; set; }
        public List<int> PageSizes { get; set; } = new List<int> { 10, 20, 30 };

        public int TotalEmployees { get; set; }


        
        public List<Department> Departments { get; set; }

        [BindProperty]
        public int? EditingDepartmentID { get; set; }

        [BindProperty]
        public Department NewDepartment { get; set; }

        public int DepartmentCurrentPage { get; set; } = 1;
        public int DepartmentResultsPerPage { get; set; } = 10;
        public int DepartmentTotalPages { get; set; }

        public int DepartmentEmployees { get; set; }

        public int TotalDepartments { get; set; }




        public List<BudgetAmendment> BudgetAmendments { get; set; }

        [BindProperty]
        public int? EditingBudgetAmendmentID { get; set; }

        [BindProperty]
        public BudgetAmendment NewBudgetAmendment { get; set; }

        public int BudgetAmendmentCurrentPage { get; set; } = 1;
        public int BudgetAmendmentResultsPerPage { get; set; } = 10;
        public int BudgetAmendmentTotalPages { get; set; }

        public int TotalBudgetAmendments { get; set; }








        [BindProperty]
        [Required]
        public string SourceSpeedtype { get; set; }

        [BindProperty]
        [Required]
        public string DestinationSpeedtype { get; set; }

        [BindProperty]
        [Range(0.01, double.MaxValue, ErrorMessage = "Transfer amount must be greater than zero")]
        public decimal TransferAmount { get; set; }

        public string TransferMessage { get; set; }
        public string TransferMessageClass { get; set; }

        public async Task OnGetAsync(int pageNumber = 1, int resultsPerPage = 10, int departmentPageNumber = 1, int departmentResultsPerPage = 10, int amendmentPageNumber = 1, int amendmentResultsPerPage = 10)
        {
            EmployeeCurrentPage = pageNumber;
            EmployeeResultsPerPage = resultsPerPage;

            // Fetch employees with pagination
            var employeesQuery = _context.Employees.AsQueryable();
            TotalEmployees = await employeesQuery.CountAsync();
            EmployeeTotalPages = (int)Math.Ceiling(TotalEmployees / (double)EmployeeResultsPerPage);

            Employees = await employeesQuery
                .Skip((EmployeeCurrentPage - 1) * EmployeeResultsPerPage)
                .Take(EmployeeResultsPerPage)
                .ToListAsync();

            DepartmentCurrentPage = departmentPageNumber;
            DepartmentResultsPerPage = departmentResultsPerPage;

            // Fetch departments with pagination
            var departmentQuery = _context.Departments.AsQueryable();
            TotalDepartments = await departmentQuery.CountAsync();
            DepartmentTotalPages = (int)Math.Ceiling(TotalDepartments / (double)DepartmentResultsPerPage);

            Departments = await departmentQuery
                .Skip((DepartmentCurrentPage - 1) * DepartmentResultsPerPage)
                .Take(DepartmentResultsPerPage)
                .ToListAsync();

            BudgetAmendmentCurrentPage = amendmentPageNumber;
            BudgetAmendmentResultsPerPage = amendmentResultsPerPage;

            // Fetch amendments with pagination
            var amendmentQuery = _context.BudgetAmendments.AsQueryable();
            TotalBudgetAmendments = await amendmentQuery.CountAsync();
            BudgetAmendmentTotalPages = (int)Math.Ceiling(TotalBudgetAmendments / (double)BudgetAmendmentResultsPerPage);

            BudgetAmendments = await amendmentQuery
            .Skip((BudgetAmendmentCurrentPage - 1) * BudgetAmendmentResultsPerPage)
            .Take(BudgetAmendmentResultsPerPage)
            .ToListAsync();
        }

        public async Task<IActionResult> OnPostAddEmployeeAsync()
        {
            if (!ModelState.IsValid)
            {
                Debug.WriteLine("======== INVALID MODEL =========");
/*                Employees = await _context.Employees.ToListAsync(); // Re-fetch employees to display on the page
                Departments = await _context.Departments.ToListAsync();  // Re-fetch departments to display on the page
                return Page();*/
            }

            _context.Employees.Add(NewEmployee);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditEmployeeAsync(int id)
        {
            EditingEmployeeID = id;

            Employees = await _context.Employees.ToListAsync();
            Departments = await _context.Departments.ToListAsync();
            BudgetAmendments = await _context.BudgetAmendments.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCancelEditEmployeeAsync(int id)
        {
            EditingEmployeeID = 0;

            Employees = await _context.Employees.ToListAsync();
            Departments = await _context.Departments.ToListAsync();
            BudgetAmendments = await _context.BudgetAmendments.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostSaveEmployeeAsync()
        {

            if (!ModelState.IsValid)
            {
/*                Employees = await _context.Employees.ToListAsync();
                Departments = await _context.Departments.ToListAsync();
                return Page();*/
            }

            var employee = await _context.Employees.FindAsync(NewEmployee.EmployeeID);


            if (employee != null)
            {
                employee.FirstName = NewEmployee.FirstName;
                employee.LastName = NewEmployee.LastName;
                employee.DateOfBirth = NewEmployee.DateOfBirth;
                employee.Email = NewEmployee.Email;
                employee.PhoneNumber = NewEmployee.PhoneNumber;
                employee.HireDate = NewEmployee.HireDate;
                employee.JobTitle = NewEmployee.JobTitle;
                employee.Salary = NewEmployee.Salary;
                employee.DepartmentID = NewEmployee.DepartmentID;

                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteEmployeeAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddDepartmentAsync()
        {

            Debug.WriteLine("======== ADD DEPT ===========");

            if (!ModelState.IsValid)
            {

                Debug.WriteLine("======== INVALID ===========");

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Debug.WriteLine(error.ErrorMessage);
                }


                Debug.WriteLine("========  ===========");

                Employees = await _context.Employees.ToListAsync(); // Re-fetch employees to display on the page
                Departments = await _context.Departments.ToListAsync();  // Re-fetch departments to display on the page
                BudgetAmendments = await _context.BudgetAmendments.ToListAsync();
                return Page();
            }

            _context.Departments.Add(NewDepartment);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }




        public async Task<IActionResult> OnPostEditDepartmentAsync(int id)
        {
            EditingDepartmentID = id;

            Employees = await _context.Employees.ToListAsync();
            Departments = await _context.Departments.ToListAsync();
            BudgetAmendments = await _context.BudgetAmendments.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCancelEditDepartmentAsync(int id)
        {
            EditingDepartmentID = 0;

            Employees = await _context.Employees.ToListAsync();
            Departments = await _context.Departments.ToListAsync();
            BudgetAmendments = await _context.BudgetAmendments.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostSaveDepartmentAsync()
        {

            ModelState.Clear();
            TryValidateModel(NewDepartment, "NewDepartment");

            if (!ModelState.IsValid)
            {
                /*                Employees = await _context.Employees.ToListAsync();
                                Departments = await _context.Departments.ToListAsync();
                                return Page();*/
            }

            var department = await _context.Departments.FindAsync(NewDepartment.DepartmentID);


            if (department != null)
            {
                department.DepartmentID = NewDepartment.DepartmentID;
                department.DepartmentName = NewDepartment.DepartmentName;
                department.Speedtype = NewDepartment.Speedtype;
                department.Budget = NewDepartment.Budget;

                await _context.SaveChangesAsync();
            }

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

            return RedirectToPage();
        }

        /*
         * 
         * Amendment
         * 
         * 
         */

        public async Task<IActionResult> OnPostAddAmendmentAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.BudgetAmendments.Add(NewBudgetAmendment);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAmendmentAsync(int id)
        {
            EditingBudgetAmendmentID = id;

            Employees = await _context.Employees.ToListAsync();
            Departments = await _context.Departments.ToListAsync();
            BudgetAmendments = await _context.BudgetAmendments.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCancelEditAmendmentAsync(int id)
        {
            EditingBudgetAmendmentID = 0;

            Employees = await _context.Employees.ToListAsync();
            Departments = await _context.Departments.ToListAsync();
            BudgetAmendments = await _context.BudgetAmendments.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAmendmentAsync()
        {
            if (!ModelState.IsValid)
            {
                // You could return the page with errors if the model is invalid.
                // For now, we'll just return the page (this part is commented out in your original code).

                BudgetAmendments = await _context.BudgetAmendments.ToListAsync();
                return Page();

            }

            // Find the existing budget amendment by ID
            var amendment = await _context.BudgetAmendments.FindAsync(NewBudgetAmendment.BudgetAmendmentID);

            if (amendment != null)
            {
                // Update the existing amendment with the new values
                amendment.BudgetAmendmentID = NewBudgetAmendment.BudgetAmendmentID;
                amendment.CategoryName = NewBudgetAmendment.CategoryName;
                amendment.AdjustmentDetail = NewBudgetAmendment.AdjustmentDetail;
                amendment.SpeedType = NewBudgetAmendment.SpeedType;
                amendment.FundCode = NewBudgetAmendment.FundCode;
                amendment.DepartmentID = NewBudgetAmendment.DepartmentID;
                amendment.ProgramCode = NewBudgetAmendment.ProgramCode;
                amendment.ClassCode = NewBudgetAmendment.ClassCode;
                amendment.AcctDescription = NewBudgetAmendment.AcctDescription;
                amendment.BudgetCode = NewBudgetAmendment.BudgetCode;
                amendment.PositionNumber = NewBudgetAmendment.PositionNumber;
                amendment.AmountIncrease = NewBudgetAmendment.AmountIncrease;
                amendment.AmountDecrease = NewBudgetAmendment.AmountDecrease;

                // Save the changes
                await _context.SaveChangesAsync();
            }

            // Redirect back to the page after saving or updating
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAmendmentAsync(int id)
        {
            var amendment = await _context.BudgetAmendments.FindAsync(id);

            if (amendment == null)
            {
                return NotFound();
            }

            _context.BudgetAmendments.Remove(amendment);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }











        public async Task<IActionResult> OnPostTransferAsync()
        {
            var sourceDepartment = await _context.Departments.FirstOrDefaultAsync(d => d.Speedtype == SourceSpeedtype);
            var destinationDepartment = await _context.Departments.FirstOrDefaultAsync(d => d.Speedtype == DestinationSpeedtype);

            if (sourceDepartment == null)
            {
                TransferMessage = "Invalid source speedtype.";
                TransferMessageClass = "alert-danger";
                return Page();
            }

            if (destinationDepartment == null)
            {
                TransferMessage = "Invalid destination speedtype.";
                TransferMessageClass = "alert-danger";
                return Page();
            }

            if (sourceDepartment.Budget < TransferAmount)
            {
                TransferMessage = "Insufficient funds in source department.";
                TransferMessageClass = "alert-danger";
                return Page();
            }

            sourceDepartment.Budget -= TransferAmount;
            destinationDepartment.Budget += TransferAmount;

            await _context.SaveChangesAsync();

            TransferMessage = "Funds transferred successfully!";
            TransferMessageClass = "alert-success";

            Employees = await _context.Employees.ToListAsync();
            Departments = await _context.Departments.ToListAsync();

            return Page();
        }
    }
}