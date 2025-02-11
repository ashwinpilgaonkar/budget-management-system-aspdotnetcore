using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
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
        /*        private readonly SignInManager<IdentityUser> _signInManager;

                public LoginModel(SignInManager<IdentityUser> signInManager)
                {
                    _signInManager = signInManager;
                }*/

        /*private readonly IUserService _userService;*/
        private readonly CasdbtestContext _context;

/*        public LoginModel(IUserService userService)
        {
            _userService = userService;
        }*/

        public LoginModel(CasdbtestContext context)
        {
            _context = context;
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

        public string ErrorMessage { get; set; }

        /*        public IActionResult OnPost()
                {

                    var user = _userService.ValidateUser(Email, Password);
                    if (user != null)
                    {
                        // Successful login, redirect to Index
                        return RedirectToPage("/Index");
                    }

                    // Invalid login attempt
                    ErrorMessage = "Invalid username or password.";
                    return Page();
                }*/

        public IActionResult OnPost()
        {
            // Retrieve the user by email
            var user = _context.Users.SingleOrDefault(u => u.Email == Email);

            if (user == null)
            {
                // User not found
                ErrorMessage = "Invalid username or password.";
                Debug.WriteLine(ErrorMessage);
                return Page();
            }

            // Convert the stored salt back to a byte array
            byte[] salt = Convert.FromBase64String(user.Salt);

            // Hash the entered password with the stored salt
            string hashedPassword = Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: Password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                    numBytesRequested: 32));

            // Compare the hashed password with the stored hashed password
            if (hashedPassword == user.Password)
            {
                // Successful login, redirect to Index
                return RedirectToPage("/Index");
            }

            // Invalid login attempt
            ErrorMessage = "Password does not match";
            Debug.WriteLine(ErrorMessage);
            return Page();
        }

        /*
                [BindProperty]
                public InputModel Input { get; set; }

                public class InputModel
                {

                }*/

        /*        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
                {
                    if (ModelState.IsValid)
                    {
                        var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                        if (result.Succeeded)
                        {
                            return LocalRedirect(returnUrl ?? "/Index");
                        }

                        if (result.IsLockedOut)
                        {
                            return RedirectToPage("./Lockout");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                            return Page();
                        }
                    }

                    // If we got this far, something failed, redisplay form
                    return Page();
                }*/
    }
}
