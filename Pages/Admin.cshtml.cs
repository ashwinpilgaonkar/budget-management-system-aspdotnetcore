using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.Services;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class AdminModel(CasdbtestContext context, IAuthenticationService authService) : PageModel
    {
        // ==============================================
        // Instance Variables -- HELPER
        // ==============================================
        #region HELPER
        private readonly CasdbtestContext _context = context;
        private readonly IAuthenticationService _authService = authService;

            public bool isAdmin { get; set; } = false;

            public string userRole { get; set; } = "";

            public string ActiveSortTable { get; set; } = "Employee";

            public string SortColumn { get; set; } = "EmployeeID";
            public string SortOrder { get; set; } = "asc";
            public List<int> PageSizes { get; set; } = new List<int> { 10, 20, 30 };
        #endregion

        // ==============================================
        // Instance Variables -- BUDGETAMENDMENTSETTINGS
        // ==============================================
        #region BUDGETAMENDMENTSETTINGS
        [BindProperty]
            public required BudgetAmendmentSetting BudgetAmendmentSetting { get; set; }

            public DateTime BudgetAmendmentStartDate { get; set; }

            public DateTime BudgetAmendmentEndDate { get; set; }

        #endregion
        #region DATA LOADING

        public async Task LoadFormDataAsync()
            {
                isAdmin = _authService.IsAdmin(HttpContext);
                userRole = _authService.GetUserRole(HttpContext);

            // ==============================================
            //         BUDGET AMENDMENT SETTINGS DATA
            // ==============================================
            var amendmentSettings = await _context.BudgetAmendmentSettings.FirstOrDefaultAsync();
                if (amendmentSettings != null)
                {
                    BudgetAmendmentStartDate = amendmentSettings.StartDate;
                    BudgetAmendmentEndDate = amendmentSettings.EndDate;
                }
                else
                {
                    BudgetAmendmentStartDate = new DateTime(DateTime.Now.Year, 7, 1); // Example: July 1st as start
                    BudgetAmendmentEndDate = BudgetAmendmentStartDate.AddYears(1).AddDays(-1); // June 30th as end
                }
            }

            public async Task<IActionResult> OnGetAsync()
            {
                if (!_authService.IsAuthenticated(HttpContext))
                {
                    return RedirectToPage("/Login");
                }

                if (!_authService.IsAdmin(HttpContext))
                {
                    return RedirectToPage("/Index");
            }

                await LoadFormDataAsync();
                return Page();
            }

        public async Task OnGetSortColumn(string table, string column, string order)
            {
                ActiveSortTable = table;
                SortColumn = column;
                SortOrder = order;

                await LoadFormDataAsync();
            }

            public string SortOrderForColumn(string column)
            {
                return SortColumn == column && SortOrder == "asc" ? "desc" : "asc";
            }

            public string GetSortIcon(string column)
            {
                if (SortColumn == column)
                {
                    return SortOrder == "asc" ? "fa-arrow-up" : "fa-arrow-down";
                }
                return "fa-sort";
            }
        #endregion

        // ==============================================
        //       BUDGET AMENDMENT SETTINGS METHODS
        // ==============================================
        #region BUDGET AMENDMENT SETTINGS METHODS
        public async Task<IActionResult> OnPostSaveBudgetAmendmentDatesAsync()
        {
            if (!ModelState.IsValid)
            {
/*                return Page();*/
            }

            var existingSetting = await _context.BudgetAmendmentSettings.FirstOrDefaultAsync();

            if (existingSetting != null)
            {
                existingSetting.StartDate = BudgetAmendmentSetting.StartDate;
                existingSetting.EndDate = BudgetAmendmentSetting.EndDate;
                existingSetting.UpdatedAt = DateTime.Now;
            }
            else
            {
                BudgetAmendmentSetting.CreatedAt = DateTime.Now;
                BudgetAmendmentSetting.UpdatedAt = DateTime.Now;
                _context.BudgetAmendmentSettings.Add(BudgetAmendmentSetting);
            }

            await _context.SaveChangesAsync();
            await LoadFormDataAsync();
            return RedirectToPage();
        }
        #endregion
    }
}
