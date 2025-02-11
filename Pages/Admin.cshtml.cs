using budget_management_system_aspdotnetcore.Entities;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class AdminModel : PageModel
    {
        private readonly CasdbtestContext _context;
        public List<User> Users { get; set; }

        [BindProperty]
        public User NewUser { get; set; }

        [BindProperty]
        public int? EditingUserId { get; set; }

        [BindProperty]
        public BudgetAmendmentSetting BudgetAmendmentSetting { get; set; }

        public DateTime BudgetAmendmentStartDate { get; set; }

        public DateTime BudgetAmendmentEndDate { get; set; }

        public AdminModel(CasdbtestContext context)
        {
            _context = context;
        }

        public async Task LoadFormDataAsync()
        {
            Users = await _context.Users.ToListAsync();

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

        public async Task OnGetAsync()
        {
            await LoadFormDataAsync();
        }

        public async Task<IActionResult> OnPostAddUserAsync()
        {
            if (!ModelState.IsValid)
            {
                /*                Employees = await _context.Employees.ToListAsync(); // Re-fetch employees to display on the page
                                Departments = await _context.Departments.ToListAsync();  // Re-fetch departments to display on the page
                                return Page();*/
            }

            byte[] salt = new byte[16];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            // Hash the password using PBKDF2
            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: NewUser.Password, // Plain-text password from form
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));

            // Store the salt and hashed password in the database
            NewUser.Password = hashedPassword; // Save hashed password
            NewUser.Salt = Convert.ToBase64String(salt); // Store the salt

            _context.Users.Add(NewUser);
            await _context.SaveChangesAsync();

            await LoadFormDataAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditUserAsync(int id)
        {
            EditingUserId = id;

            await LoadFormDataAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCancelEditUserAsync(int id)
        {
            EditingUserId = 0;

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
                user.Password = NewUser.Password;

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

        public async Task<IActionResult> OnPostSaveBudgetAmendmentDatesAsync()
        {
            if (!ModelState.IsValid)
            {
/*                return Page();*/
            }

            var existingSetting = await _context.BudgetAmendmentSettings.FirstOrDefaultAsync();

            if (existingSetting != null)
            {
                // Update existing budget amendment setting
                existingSetting.StartDate = BudgetAmendmentSetting.StartDate;
                existingSetting.EndDate = BudgetAmendmentSetting.EndDate;
                existingSetting.UpdatedAt = DateTime.Now; // Update timestamp
            }
            else
            {
                // Create new budget amendment setting
                BudgetAmendmentSetting.CreatedAt = DateTime.Now; // Set creation timestamp
                BudgetAmendmentSetting.UpdatedAt = DateTime.Now; // Set update timestamp
                _context.BudgetAmendmentSettings.Add(BudgetAmendmentSetting);
            }

            await _context.SaveChangesAsync();

            await LoadFormDataAsync();

            return RedirectToPage();
        }
    }
}
