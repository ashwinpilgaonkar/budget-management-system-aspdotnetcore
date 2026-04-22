using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.Services;
using budget_management_system_aspdotnetcore.ViewModels;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class ActivityLogModel : PageModel
    {
        private readonly CasdbtestContext _context;
        private readonly IAuthenticationService _authService;

        public string userRole { get; set; } = "";
        public List<UserActivityLog> UserActivityLogs { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int ResultsPerPage { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }

        public ActivityLogModel(CasdbtestContext context, IAuthenticationService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1, int resultsPerPage = 10)
        {
            if (!_authService.IsAuthenticated(HttpContext))
            {
                return RedirectToPage("/Login");
            }

            CurrentPage = pageNumber;
            ResultsPerPage = resultsPerPage;
            await LoadFormDataAsync();
            return Page();
        }

        public async Task LoadFormDataAsync()
        {
            userRole = _authService.GetUserRole(HttpContext);

            var query = _context.UserActivityLogs
                .Include(log => log.User)
                .OrderByDescending(log => log.Timestamp);

            TotalRecords = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalRecords / (double)ResultsPerPage);

            UserActivityLogs = await query
                .Skip((CurrentPage - 1) * ResultsPerPage)
                .Take(ResultsPerPage)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostExportToExcelActivityLogAsync()
        {
            var logs = await _context.UserActivityLogs
                .Include(log => log.User)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Activity Log");

            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "User";
            worksheet.Cell(1, 3).Value = "Category";
            worksheet.Cell(1, 4).Value = "Description";
            worksheet.Cell(1, 5).Value = "Timestamp";

            var headerRange = worksheet.Range(1, 1, 1, 5);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontColor = XLColor.Black;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#DCE6F1");
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.SheetView.FreezeRows(1);

            int row = 2;
            int serialNo = 1;
            foreach (var log in logs)
            {
                worksheet.Cell(row, 1).Value = serialNo++;
                worksheet.Cell(row, 2).Value = $"{log.User.FirstName} {log.User.LastName}";
                worksheet.Cell(row, 3).Value = log.Category;
                worksheet.Cell(row, 4).Value = log.Description;
                worksheet.Cell(row, 5).Value = log.Timestamp.ToString("g");
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var date = DateTime.Now.ToString("yyyy-MM-dd");
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ActivityLog_{date}.xlsx");
        }

        public PaginationViewModel GetActivityLogPagination()
        {
            return new PaginationViewModel
            {
                CurrentPage = CurrentPage,
                TotalPages = TotalPages,
                TotalRecords = TotalRecords,
                ResultsPerPage = ResultsPerPage,
                PageSizes = PaginationViewModel.DefaultPageSizes,
                AriaLabel = "Activity log page navigation",
                PrevUrl = $"?pageNumber={CurrentPage - 1}&resultsPerPage={ResultsPerPage}",
                NextUrl = $"?pageNumber={CurrentPage + 1}&resultsPerPage={ResultsPerPage}",
                SizeChangeUrlTemplate = $"?pageNumber=1&resultsPerPage=__SIZE__"
            };
        }
    }
}
