using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class RolesModel(CasdbtestContext context, IAuthenticationService authService) : PageModel
    {
        private readonly CasdbtestContext _context = context;
        private readonly IAuthenticationService _authService = authService;

        public string userRole { get; set; } = "";
        public List<int> PageSizes { get; set; } = new List<int> { 10, 20, 30 };

        public IEnumerable<Role> Roles { get; set; }

        [BindProperty(SupportsGet = true)]
        public string RoleSearchTerm { get; set; }

        public int RoleCurrentPage { get; set; } = 1;
        public int RoleResultsPerPage { get; set; } = 10;
        public int RoleTotalPages { get; set; }
        public int TotalRoles { get; set; }

        public async Task LoadFormDataAsync()
        {
            userRole = _authService.GetUserRole(HttpContext);

            var roleQuery = _context.Roles.AsQueryable();

            if (!string.IsNullOrEmpty(RoleSearchTerm))
            {
                roleQuery = roleQuery.Where(r => r.RoleName.Contains(RoleSearchTerm));
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
    }
}
