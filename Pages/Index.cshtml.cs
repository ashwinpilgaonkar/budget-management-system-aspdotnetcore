using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly CasdbtestContext _context;
        private readonly IDatabaseService _databaseService;

        [Required]
        [BindProperty] // Not necessary if field is not editable
        public string JobTitle { get; set; }
        public List<Employee> employee { get; set; }

        public IndexModel(ILogger<IndexModel> logger, CasdbtestContext context, IDatabaseService databaseService)
        {
            _logger = logger;
            _context = context;
            _databaseService = databaseService;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _databaseService.AddUpdate(JobTitle);
            if (!result)
                throw new ApplicationException("error in database logic");

            return RedirectToPage("./Index");
        }

        public void OnGet()
        {
            var myData = _context.Employees.FirstOrDefault(x => x.EmployeeID == 1);
            employee = _databaseService.GetEmployeeList();
        }
    }
}
