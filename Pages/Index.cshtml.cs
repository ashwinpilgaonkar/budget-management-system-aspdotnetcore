using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.Services;
using Microsoft.AspNetCore.Authorization;
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
        public List<Employee> employees { get; set; }
        public List<Department> departments { get; set; }  // Add this line

        public IndexModel(CasdbtestContext context)
        {
            _context = context;
        }

        public IList<Employee> Employees { get; set; }
        public IList<Department> Departments { get; set; }  // Add this line

        [BindProperty]
        public Employee NewEmployee { get; set; }

        [BindProperty]
        public Department NewDepartment { get; set; }

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

        public async Task OnGetAsync()
        {
            employees = await _context.Employees.ToListAsync();
            departments = await _context.Departments.ToListAsync();  // Fetch departments
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                employees = await _context.Employees.ToListAsync(); // Re-fetch employees to display on the page
                departments = await _context.Departments.ToListAsync();  // Re-fetch departments to display on the page
                return Page();
            }

            _context.Employees.Add(NewEmployee);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
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

        // Handling fund transfer
        public async Task<IActionResult> OnPostTransferAsync()
        {
            Debug.WriteLine("=======ON POST TRANSFER============");

/*            if (!ModelState.IsValid)
            {
                Debug.WriteLine("=======MODEL INVALID============");

                employees = await _context.Employees.ToListAsync();
                departments = await _context.Departments.ToListAsync();
                return Page();
            }*/

            // Fetch the source and destination departments
            var sourceDepartment = await _context.Departments.FirstOrDefaultAsync(d => d.Speedtype == SourceSpeedtype);
            var destinationDepartment = await _context.Departments.FirstOrDefaultAsync(d => d.Speedtype == DestinationSpeedtype);

            // Validate source and destination
            if (sourceDepartment == null)
            {

                Debug.WriteLine("=======SOURCE INVALID============");

                TransferMessage = "Invalid source speedtype.";
                TransferMessageClass = "alert-danger";
                return Page();
            }

            if (destinationDepartment == null)
            {

                Debug.WriteLine("=======DESTINATION INVALID============");

                TransferMessage = "Invalid destination speedtype.";
                TransferMessageClass = "alert-danger";
                return Page();
            }

            Debug.WriteLine("========BUDGET========");
            Debug.WriteLine(sourceDepartment.Budget);
            Debug.WriteLine(TransferAmount);

            // Check if source has sufficient funds
            if (sourceDepartment.Budget < TransferAmount)
            {
                TransferMessage = "Insufficient funds in source department.";
                TransferMessageClass = "alert-danger";
                return Page();
            }

            // Perform the fund transfer
            sourceDepartment.Budget -= TransferAmount;
            destinationDepartment.Budget += TransferAmount;

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Success message
            TransferMessage = "Funds transferred successfully!";
            TransferMessageClass = "alert-success";

            // Re-fetch data for display
            employees = await _context.Employees.ToListAsync();
            departments = await _context.Departments.ToListAsync();

            return Page();
        }
    }
}