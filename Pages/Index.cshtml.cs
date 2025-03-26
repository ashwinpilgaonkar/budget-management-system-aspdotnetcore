using budget_management_system_aspdotnetcore.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using budget_management_system_aspdotnetcore.Services;

namespace budget_management_system_aspdotnetcore.Pages
{
    public class IndexModel : PageModel
    {
        // ==============================================
        // Instance Variables -- HELPER
        // ==============================================
        #region HELPER
            private readonly CasdbtestContext _context;
            private readonly SpeedTypeService _speedTypeService;


        #endregion


        public string ActiveSortTable { get; set; } = "Department";

        public string SortColumn { get; set; } = "DepartmentID";
        public string SortOrder { get; set; } = "asc";

        public List<int> PageSizes { get; set; } = new List<int> { 10, 20, 30 };

        public IEnumerable<SpeedType> SpeedTypes { get; set; }

        public List<BudgetAmendment> BudgetAmendments { get; set; }

        [BindProperty]
        public int? EditingBudgetAmendmentID { get; set; }

        [BindProperty]
        public BudgetAmendment NewBudgetAmendment { get; set; }

        public int BudgetAmendmentCurrentPage { get; set; } = 1;
        public int BudgetAmendmentResultsPerPage { get; set; } = 10;
        public int BudgetAmendmentTotalPages { get; set; }

        public int TotalBudgetAmendments { get; set; }

        [BindProperty(SupportsGet = true)]
        public string BudgetAmendmentSearchTerm { get; set; }

        [BindProperty]
        [Required]
        public double AmountTotal { get; set; }

        [BindProperty]
        [Required]
        public int SourceSpeedtype { get; set; }

        [BindProperty]
        [Required]
        public int DestinationSpeedtype { get; set; }

        public DateTime BudgetAmendmentStartDate { get; set; }
        public DateTime BudgetAmendmentEndDate { get; set; }

        [BindProperty]
        [Range(0.01, double.MaxValue, ErrorMessage = "Transfer amount must be greater than zero")]
        public double TransferAmount { get; set; }

        public IndexModel(CasdbtestContext context, SpeedTypeService speedTypeService)
        {
            _context = context;
            _speedTypeService = speedTypeService;
        }

        public async Task LoadFormDataAsync()
        {
            SpeedTypes = await _speedTypeService.GetSpeedTypesAsync();

            //Fetch BudgetAmendment Data
            var amendmentQuery = _context.BudgetAmendments.AsQueryable();

            if (!string.IsNullOrEmpty(BudgetAmendmentSearchTerm))
            {
                amendmentQuery = amendmentQuery.Where(s => s.CategoryName.Contains(BudgetAmendmentSearchTerm)
                || s.AdjustmentDetail.ToString().Contains(BudgetAmendmentSearchTerm)
                || s.SpeedType.Code.ToString().Contains(BudgetAmendmentSearchTerm)
                || s.FundCode.ToString().Contains(BudgetAmendmentSearchTerm)
                || s.DepartmentID.ToString().Contains(BudgetAmendmentSearchTerm)
                || s.ProgramCode.ToString().Contains(BudgetAmendmentSearchTerm)
                || s.ClassCode.ToString().Contains(BudgetAmendmentSearchTerm)
                || s.AcctDescription.ToString().Contains(BudgetAmendmentSearchTerm)
                || s.BudgetCode.ToString().Contains(BudgetAmendmentSearchTerm)
                || s.PositionNumber.ToString().Contains(BudgetAmendmentSearchTerm)
                || s.AmountIncrease.ToString().Contains(BudgetAmendmentSearchTerm)
                || s.AmountDecrease.ToString().Contains(BudgetAmendmentSearchTerm));
            }

            if (!string.IsNullOrEmpty(SortColumn) && ActiveSortTable == "AmendmentHistory")
            {
                amendmentQuery = SortOrder == "asc"
                    ? amendmentQuery.OrderBy(e => EF.Property<string>(e, SortColumn).ToString())
                    : amendmentQuery.OrderByDescending(e => EF.Property<string>(e, SortColumn).ToString());

            }

            TotalBudgetAmendments = await amendmentQuery.CountAsync();
            BudgetAmendmentTotalPages = (int)Math.Ceiling(TotalBudgetAmendments / (double)BudgetAmendmentResultsPerPage);

            BudgetAmendments = await amendmentQuery
            .Include(b => b.CreatedByUser)
            .Skip((BudgetAmendmentCurrentPage - 1) * BudgetAmendmentResultsPerPage)
            .Take(BudgetAmendmentResultsPerPage)
            .ToListAsync();


            //Fetch Budget Amendment Settings
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


        public async Task OnGetAsync(int amendmentPageNumber = 1, int amendmentResultsPerPage = 10)
        {
            BudgetAmendmentCurrentPage = amendmentPageNumber;
            BudgetAmendmentResultsPerPage = amendmentResultsPerPage;

            await LoadFormDataAsync();
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

        public async Task<IActionResult> OnPostAddAmendmentAsync()
        {
            if (!ModelState.IsValid)
            {
                /*return Page();*/
            }

            var transactionId = Guid.NewGuid();

            var budgetAmendment1 = new BudgetAmendment
            {
                CategoryName = NewBudgetAmendment.CategoryName,
                AdjustmentDetail = NewBudgetAmendment.AdjustmentDetail,
                FundCode = NewBudgetAmendment.FundCode,
                DepartmentID = NewBudgetAmendment.DepartmentID,
                ProgramCode = NewBudgetAmendment.ProgramCode,
                ClassCode = NewBudgetAmendment.ClassCode,
                AcctDescription = NewBudgetAmendment.AcctDescription,
                BudgetCode = NewBudgetAmendment.BudgetCode,
                PositionNumber = NewBudgetAmendment.PositionNumber,
                TransactionId = transactionId,

                SpeedTypeId = DestinationSpeedtype,
                AmountIncrease = TransferAmount,
                AmountDecrease = 0,

                CreatedBy = 1,
                UpdatedBy = 1,
                EditedBy = 1,
                EditedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var budgetAmendment2 = new BudgetAmendment
            {
                CategoryName = NewBudgetAmendment.CategoryName,
                AdjustmentDetail = NewBudgetAmendment.AdjustmentDetail,
                FundCode = NewBudgetAmendment.FundCode,
                DepartmentID = NewBudgetAmendment.DepartmentID,
                ProgramCode = NewBudgetAmendment.ProgramCode,
                ClassCode = NewBudgetAmendment.ClassCode,
                AcctDescription = NewBudgetAmendment.AcctDescription,
                BudgetCode = NewBudgetAmendment.BudgetCode,
                PositionNumber = NewBudgetAmendment.PositionNumber,
                TransactionId = transactionId,

                SpeedTypeId = SourceSpeedtype,
                AmountIncrease = 0,
                AmountDecrease = TransferAmount,

                CreatedBy = 1,
                UpdatedBy = 1,
                EditedBy = 1,
                EditedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.BudgetAmendments.Add(budgetAmendment1);
            _context.BudgetAmendments.Add(budgetAmendment2);
            await _context.SaveChangesAsync();

            await LoadFormDataAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAmendmentAsync(int id)
        {
            EditingBudgetAmendmentID = id;

            var amendment = await _context.BudgetAmendments.FindAsync(id);

            if (amendment != null)
            {
                var relatedAmendments = await _context.BudgetAmendments
                    .Where(a => a.TransactionId == amendment.TransactionId)
                    .ToListAsync();

                if (relatedAmendments.Count == 2)
                {
                    // Identify source and destination amendments in the pair 
                    var sourceAmendment = relatedAmendments.FirstOrDefault(a => a.AmountIncrease == 0);
                    var destinationAmendment = relatedAmendments.FirstOrDefault(a => a.AmountDecrease == 0);

                    if (sourceAmendment != null && destinationAmendment != null)
                    {
                        SourceSpeedtype = sourceAmendment.SpeedTypeId;
                        DestinationSpeedtype = destinationAmendment.SpeedTypeId;
                    }
                }
            }



/*            if(amendment != null && amendment.AmountDecrease == 0)
            {
                SelectedSpeedType = amendment.SpeedTypeId;
            }*/

            await LoadFormDataAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCancelEditAmendmentAsync(int id)
        {
            EditingBudgetAmendmentID = 0;

            await LoadFormDataAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAmendmentAsync()
        {

            /*            if (!ModelState.IsValid)
                        {
                            await LoadFormDataAsync();
                            return Page(); // Return the same page to display validation errors
                        }*/

            var amendment = await _context.BudgetAmendments.FindAsync(NewBudgetAmendment.BudgetAmendmentID);

            if (amendment != null)
            {
                var relatedAmendments = await _context.BudgetAmendments
                    .Where(a => a.TransactionId == amendment.TransactionId)
                    .ToListAsync();

                if (relatedAmendments.Count == 2)
                {
                    // Identify source and destination amendments in the pair 
                    var sourceAmendment = relatedAmendments.FirstOrDefault(a => a.SpeedTypeId == SourceSpeedtype);
                    var destinationAmendment = relatedAmendments.FirstOrDefault(a => a.SpeedTypeId == DestinationSpeedtype);

                    if (sourceAmendment == null || destinationAmendment == null)
                    {
                        destinationAmendment = relatedAmendments[0];
                        sourceAmendment = relatedAmendments[1];

                        sourceAmendment.SpeedTypeId = SourceSpeedtype;
                        destinationAmendment.SpeedTypeId = DestinationSpeedtype;
                    }

                    foreach (var relatedAmendment in relatedAmendments)
                    {
                        relatedAmendment.CategoryName = NewBudgetAmendment.CategoryName;
                        relatedAmendment.AdjustmentDetail = NewBudgetAmendment.AdjustmentDetail;
                        relatedAmendment.FundCode = NewBudgetAmendment.FundCode;
                        relatedAmendment.DepartmentID = NewBudgetAmendment.DepartmentID;
                        relatedAmendment.ProgramCode = NewBudgetAmendment.ProgramCode;
                        relatedAmendment.ClassCode = NewBudgetAmendment.ClassCode;
                        relatedAmendment.AcctDescription = NewBudgetAmendment.AcctDescription;
                        relatedAmendment.BudgetCode = NewBudgetAmendment.BudgetCode;
                        relatedAmendment.PositionNumber = NewBudgetAmendment.PositionNumber;

                        relatedAmendment.EditedAt = DateTime.Now;
                        relatedAmendment.EditedBy = 1; // Set to current user's ID once available
                    }

                    sourceAmendment.AmountIncrease = NewBudgetAmendment.AmountIncrease;
                    sourceAmendment.AmountDecrease = NewBudgetAmendment.AmountDecrease;

                    destinationAmendment.AmountIncrease = NewBudgetAmendment.AmountIncrease;
                    destinationAmendment.AmountDecrease = NewBudgetAmendment.AmountDecrease;

                    await _context.SaveChangesAsync();
                }
            }

            await LoadFormDataAsync();

            return RedirectToPage();
        }



        public async Task<IActionResult> OnPostDeleteAmendmentAsync(int id)
        {
            var amendment = await _context.BudgetAmendments.FindAsync(id);

            if (amendment == null)
            {
                return NotFound();
            }

            var transactionId = amendment.TransactionId;

            var relatedAmendments = _context.BudgetAmendments
                .Where(a => a.TransactionId == transactionId);

            _context.BudgetAmendments.RemoveRange(relatedAmendments);
            await _context.SaveChangesAsync();

            await LoadFormDataAsync();

            return RedirectToPage();
        }

        /*        public async Task<IActionResult> OnPostApproveAmendmentAsync(int id)
                {
                    var budgetAmendment = await _context.BudgetAmendments.FindAsync(id);

                    if (budgetAmendment == null)
                    {
                        return NotFound();
                    }

                    budgetAmendment.UpdatedBy = 1;
                    budgetAmendment.UpdatedAt = DateTime.Now;

                    if(budgetAmendment.AmountIncrease != 0)
                    {
                        budgetAmendment.SpeedType.Budget = budgetAmendment.SpeedType.Budget + budgetAmendment.AmountIncrease;
                    } 
                    else
                    {

                    }

                    await OnPostUpdateAmendmentStatusAsync(budgetAmendment.TransactionId, AmendmentStatus.Approved);

                    return RedirectToPage();
                }*/

        /*        public async Task<IActionResult> OnPostApproveAmendmentAsync(int id)
                {
                    // Retrieve the initial budget amendment
                    var budgetAmendment = await _context.BudgetAmendments.FindAsync(id);

                    if (budgetAmendment == null)
                    {
                        return NotFound();
                    }

                    // Retrieve all amendments with the same TransactionId (should return a pair)
                    var amendmentPair = await _context.BudgetAmendments
                        .Where(a => a.TransactionId == budgetAmendment.TransactionId)
                        .Include(a => a.SpeedType) // Include SpeedType for accessing its Budget
                        .ToListAsync();

                    if (amendmentPair.Count != 2)
                    {
                        // Handle the case if there aren't exactly two amendments
                        return BadRequest("Amendment pair not found.");
                    }

                    // Determine source and destination amendments
                    var destinationAmendment = amendmentPair.FirstOrDefault(a => a.AmountIncrease != 0);
                    var sourceAmendment = amendmentPair.FirstOrDefault(a => a.AmountDecrease != 0);

                    if (destinationAmendment == null || sourceAmendment == null)
                    {
                        return BadRequest("Invalid amendment pair.");
                    }

                    // Update the Budget values
                    destinationAmendment.SpeedType.Budget += (decimal)destinationAmendment.AmountIncrease;
                    sourceAmendment.SpeedType.Budget -= (decimal)sourceAmendment.AmountDecrease;

                    // Update status and metadata for both amendments
                    foreach (var amendment in amendmentPair)
                    {
                        amendment.Status = AmendmentStatus.Approved;
                        amendment.UpdatedBy = 1;  // Set to current user's ID once available
                        amendment.UpdatedAt = DateTime.Now;
                    }

                    // Save all changes
                    await _context.SaveChangesAsync();

                    return RedirectToPage();
                }*/

        public async Task<IActionResult> OnPostApproveCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                return BadRequest("Invalid category");

            var amendments = _context.BudgetAmendments.Where(a => a.CategoryName == category);

            foreach (var amendment in amendments)
            {
                amendment.Status = AmendmentStatus.Approved;
                amendment.UpdatedBy = 1; 
                amendment.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                return BadRequest("Invalid category");

            var amendments = _context.BudgetAmendments.Where(a => a.CategoryName == category);

            foreach (var amendment in amendments)
            {
                amendment.Status = AmendmentStatus.Rejected;
                amendment.UpdatedBy = 1;
                amendment.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        /*        public async Task<IActionResult> OnPostRejectAmendmentAsync(int id)
                {

                    var budgetAmendment = await _context.BudgetAmendments.FindAsync(id);

                    if (budgetAmendment == null)
                    {
                        return NotFound();
                    }

                    budgetAmendment.UpdatedBy = 1;
                    budgetAmendment.UpdatedAt = DateTime.Now;

                    await OnPostUpdateAmendmentStatusAsync(budgetAmendment.TransactionId, AmendmentStatus.Rejected);

                    return RedirectToPage();
                }*/

        public async Task<IActionResult> OnPostUpdateAmendmentStatusAsync(Guid transactionId, AmendmentStatus newStatus)
        {
            // Retrieve all amendments with the specified TransactionId
            var budgetAmendments = await _context.BudgetAmendments
                .Where(ba => ba.TransactionId == transactionId)
                .ToListAsync();

            // Check if two related amendments exist
            if (budgetAmendments.Count != 2)
            {
                return NotFound();
            }

            // Update the status of both amendments
            foreach (var amendment in budgetAmendments)
            {
                amendment.Status = newStatus;

                amendment.UpdatedAt = DateTime.Now;
                amendment.UpdatedBy = 1; // Set to current user's ID once available
            }

            await _context.SaveChangesAsync();
            await LoadFormDataAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExportToExcelBudgetAmendmentsAsync()
        {
            var budgetAmendments = await _context.BudgetAmendments
                .Include(ba => ba.SpeedType) // Include related SpeedType information
                .Include(cb => cb.CreatedByUser)
                .ToListAsync();

            Debug.WriteLine(budgetAmendments[0].CreatedByUser?.Email);


            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Budget Amendments");

            // Define headers
            worksheet.Cell(1, 1).Value = "Budget Amendment ID";
            worksheet.Cell(1, 2).Value = "Category Name";
            worksheet.Cell(1, 3).Value = "Adjustment Detail";
            worksheet.Cell(1, 4).Value = "SpeedType ID";
            worksheet.Cell(1, 5).Value = "SpeedType Code";
            worksheet.Cell(1, 6).Value = "Fund Code";
            worksheet.Cell(1, 7).Value = "Department ID";
            worksheet.Cell(1, 8).Value = "Program Code";
            worksheet.Cell(1, 9).Value = "Class Code";
            worksheet.Cell(1, 10).Value = "Acct Description";
            worksheet.Cell(1, 11).Value = "Budget Code";
            worksheet.Cell(1, 12).Value = "Position Number";
            worksheet.Cell(1, 13).Value = "Amount Increase";
            worksheet.Cell(1, 14).Value = "Amount Decrease";
            worksheet.Cell(1, 15).Value = "Transaction ID";
            worksheet.Cell(1, 16).Value = "Status";
            worksheet.Cell(1, 17).Value = "Created At";
            worksheet.Cell(1, 18).Value = "Updated At";
            worksheet.Cell(1, 19).Value = "Edited At";
            worksheet.Cell(1, 20).Value = "Created By";
            worksheet.Cell(1, 21).Value = "Edited By";
            worksheet.Cell(1, 22).Value = "Updated By";

            // Populate data
            int row = 2;
            foreach (var amendment in budgetAmendments)
            {
                worksheet.Cell(row, 1).Value = amendment.BudgetAmendmentID;
                worksheet.Cell(row, 2).Value = amendment.CategoryName;
                worksheet.Cell(row, 3).Value = amendment.AdjustmentDetail;
                worksheet.Cell(row, 4).Value = amendment.SpeedTypeId;
                worksheet.Cell(row, 5).Value = amendment.SpeedType?.Code;
                worksheet.Cell(row, 6).Value = amendment.FundCode;
                worksheet.Cell(row, 7).Value = amendment.DepartmentID;
                worksheet.Cell(row, 8).Value = amendment.ProgramCode;
                worksheet.Cell(row, 9).Value = amendment.ClassCode;
                worksheet.Cell(row, 10).Value = amendment.AcctDescription;
                worksheet.Cell(row, 11).Value = amendment.BudgetCode;
                worksheet.Cell(row, 12).Value = amendment.PositionNumber;
                worksheet.Cell(row, 13).Value = amendment.AmountIncrease;
                worksheet.Cell(row, 14).Value = amendment.AmountDecrease;
                worksheet.Cell(row, 15).Value = amendment.TransactionId.ToString();
                worksheet.Cell(row, 16).Value = amendment.Status.ToString();
                worksheet.Cell(row, 17).Value = amendment.CreatedAt?.ToString("MM/dd/yyyy");
                worksheet.Cell(row, 18).Value = amendment.UpdatedAt?.ToString("MM/dd/yyyy");
                worksheet.Cell(row, 19).Value = amendment.EditedAt?.ToString("MM/dd/yyyy");
                worksheet.Cell(row, 20).Value = amendment.CreatedByUser?.Email;
                worksheet.Cell(row, 21).Value = amendment.EditedByUser?.Email;
                worksheet.Cell(row, 22).Value = amendment.UpdatedByUser?.Email;
                row++;
            }

            // Save the workbook to a memory stream
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BudgetAmendments.xlsx");
        }

    }
}