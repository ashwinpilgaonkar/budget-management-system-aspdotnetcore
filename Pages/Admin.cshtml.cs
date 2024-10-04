using budget_management_system_aspdotnetcore.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class AdminModel : PageModel
    {
        public List<User> Users { get; set; }

        [BindProperty]
        public User NewUser { get; set; }

        [BindProperty]
        public int? EditingUserId { get; set; }

        private readonly CasdbtestContext _context;

        public AdminModel(CasdbtestContext context)
        {
            _context = context;
        }
        public async Task OnGetAsync()
        {
            Users = await _context.Users.ToListAsync();
        }

        public async Task<IActionResult> OnPostAddUserAsync()
        {
            if (!ModelState.IsValid)
            {
                /*                Employees = await _context.Employees.ToListAsync(); // Re-fetch employees to display on the page
                                Departments = await _context.Departments.ToListAsync();  // Re-fetch departments to display on the page
                                return Page();*/
            }

            _context.Users.Add(NewUser);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditUserAsync(int id)
        {
            EditingUserId = id;

            Users = await _context.Users.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCancelEditUserAsync(int id)
        {
            EditingUserId = 0;

            Users = await _context.Users.ToListAsync();

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
                user.Password = NewUser.Password;

                await _context.SaveChangesAsync();
            }

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

            return RedirectToPage();
        }
    }
}
