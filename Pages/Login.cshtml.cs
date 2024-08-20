using budget_management_system_aspdotnetcore.Services;
using Microsoft.AspNetCore.Authentication;
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

        private readonly IUserService _userService;

        public LoginModel(IUserService userService)
        {
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

        public string ErrorMessage { get; set; }

        public IActionResult OnPost()
        {
            // You can add your login logic here. 
            // Since this is a dummy page, we'll just redirect to Index.

            Debug.WriteLine("====== POST ========");
            Debug.WriteLine(Email);
            Debug.WriteLine(Password);

            var user = _userService.ValidateUser(Email, Password);
            if (user != null)
            {

                Debug.WriteLine("REDIRECTING TO INDEX");

                // Successful login, redirect to Index
                return RedirectToPage("/Index");
            }

            Debug.WriteLine("INVALID");

            // Invalid login attempt
            ErrorMessage = "Invalid username or password.";
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
