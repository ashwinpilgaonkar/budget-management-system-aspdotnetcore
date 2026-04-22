using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;

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

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true || !string.IsNullOrEmpty(HttpContext.Session.GetString("Email")))
                return RedirectToPage("/Index");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == Email);

            if (user == null)
            {
                ErrorMessage = "User does not exist";
                Debug.WriteLine(ErrorMessage);
                return Page();
            }

            var role = _context.Roles.SingleOrDefault(r => r.RoleID == user.RoleID);

            if (role == null)
            {
                ErrorMessage = "Invalid user. Does not have assigned role.";
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
                HttpContext.Session.SetString("RoleName", role.RoleName);

                if (RememberMe)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim(ClaimTypes.Email,          user.Email),
                        new Claim(ClaimTypes.GivenName,      user.FirstName),
                        new Claim(ClaimTypes.Surname,        user.LastName),
                        new Claim("RoleID",                  user.RoleID.ToString()),
                        new Claim(ClaimTypes.Role,           role.RoleName)
                    };
                    var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    var authProps = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc   = DateTimeOffset.UtcNow.AddDays(30)
                    };
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);
                }

                return RedirectToPage("/Index");
            }

            // Invalid login attempt
            ErrorMessage = "Invalid password";
            Debug.WriteLine(ErrorMessage);
            return Page();
        }
    }
}
