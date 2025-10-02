using budget_management_system_aspdotnetcore.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace budget_management_system_aspdotnetcore.Entities
{
    public class BudgetAmendmentMain
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BudgetAmendmentMainID { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        [ForeignKey("CreatedByUser")]
        public int CreatedBy { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Required]
        [ForeignKey("UpdatedByUser")]
        public int UpdatedBy { get; set; }

        // Navigation properties
        public virtual User CreatedByUser { get; set; }
        public virtual User UpdatedByUser { get; set; }
    }
}