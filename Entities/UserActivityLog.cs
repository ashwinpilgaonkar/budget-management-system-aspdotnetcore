namespace budget_management_system_aspdotnetcore.Entities
{
    public partial class UserActivityLog
    {
        public int UserActivityLogID { get; set; } // Primary Key

        public required int UserID { get; set; } // Foreign key
        public required string Category { get; set; }
        public required string Description { get; set; }
        public required DateTime Timestamp { get; set; }

        public virtual User User { get; set; } = null!;
    }
    public enum ActivityType
    {
        Submitted,
        Approved,
        Rejected,
        Withdrawn,
        Reverted,
        Edited,
        Deleted
    }
}
