using System.ComponentModel.DataAnnotations.Schema;

namespace budget_management_system_aspdotnetcore.Entities
{
    public partial class BudgetAmendment
    {
        public int BudgetAmendmentID { get; set; }
        public string CategoryName { get; set; }
        public string AdjustmentDetail { get; set; }
        public string SpeedType { get; set; }
        public string FundCode { get; set; }
        public string DepartmentID { get; set; }
        public string ProgramCode { get; set; }
        public string ClassCode { get; set; }
        public string AcctDescription { get; set; }
        public string BudgetCode { get; set; }
        public string PositionNumber { get; set; }
        public double AmountIncrease { get; set; }
        public double AmountDecrease { get; set; }
    }
}