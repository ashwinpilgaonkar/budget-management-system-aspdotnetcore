using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            HttpContext.Session.Clear();  // Clear session data
            return RedirectToPage("/Login");  // Redirect to Login page
        }
    }
}
