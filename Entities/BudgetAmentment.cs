using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace budget_management_system_aspdotnetcore.Entities
{
    public partial class BudgetAmendment
    {
        public int BudgetAmendmentID { get; set; }
        public required string CategoryName { get; set; }
        public required string AdjustmentDetail { get; set; }
        public int SpeedTypeId { get; set; }
        public required string FundCode { get; set; }
        public required int DepartmentID { get; set; }
        public required string ProgramCode { get; set; }
        public required string ClassCode { get; set; }
        public required string AcctDescription { get; set; }
        public required string BudgetCode { get; set; }
        public required string PositionNumber { get; set; }
        public double AmountIncrease { get; set; }
        public double AmountDecrease { get; set; }

        public SpeedType SpeedType { get; set; }
        public Guid TransactionId { get; set; }
        public AmendmentStatus Status { get; set; } = AmendmentStatus.Draft;

        // New audit columns
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? EditedAt { get; set; }

        public int? CreatedBy { get; set; }
        public int? EditedBy { get; set; }
        public int? UpdatedBy { get; set; }

        [Required]
        [ForeignKey("BudgetAmendmentMain")]
        public int BudgetAmendmentMainID { get; set; }

        public virtual BudgetAmendmentMain BudgetAmendmentMain { get; set; }

        // Foreign key relationships
        [ForeignKey("CreatedBy")]
        public User? CreatedByUser { get; set; }

        [ForeignKey("EditedBy")]
        public User? EditedByUser { get; set; }

        [ForeignKey("UpdatedBy")]
        public User? UpdatedByUser { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        [Required]
        [DefaultValue(0)]
        public int ExtensionDays { get; set; }
    }

    public enum AmendmentStatus
    {
        Pending,
        Approved,
        Rejected,
        Draft
    }
}