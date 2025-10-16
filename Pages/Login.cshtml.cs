using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class LoginModel : PageModel
    {
        private readonly UserService _userService;
        private readonly CasdbtestContext _context;

        public LoginModel(CasdbtestContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [BindProperty]
        [Required]
        [EmailAddress]
        public string Email { get; set; }
         
        [BindProperty]
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [BindProperty]
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == Email);

            if (user == null)
            {
                ErrorMessage = "User does not exist";
                Debug.WriteLine(ErrorMessage);
                return Page();
            }

            // Compare entered password with stored hash
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(Password, user.Password);
            bool isActiveUser = user.Status == UserStatus.active;

            if (isPasswordValid && isActiveUser)
            {
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Email", user.Email);
                HttpContext.Session.SetString("FirstName", user.FirstName);
                HttpContext.Session.SetString("LastName", user.LastName);
                HttpContext.Session.SetString("RoleID", user.RoleID.ToString());

                // Successful login, redirect to Index
                return RedirectToPage("/Index");
            }

            // Invalid login attempt
            ErrorMessage = "Invalid password";
            Debug.WriteLine(ErrorMessage);
            return Page();
        }
    }
}
