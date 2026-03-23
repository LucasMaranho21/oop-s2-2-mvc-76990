using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodSafetyInspectionTracker.Models
{
    public class Inspection
    {
        public int Id { get; set; }

        [Required]
        public int PremisesId { get; set; }

        [ForeignKey("PremisesId")]
        public Premises? Premises { get; set; }

        [Required]
        public DateTime InspectionDate { get; set; }

        [Range(0, 100)]
        public int Score { get; set; }

        [Required]
        [StringLength(20)]
        public string Outcome { get; set; } = "Pass";

        [StringLength(500)]
        public string? Notes { get; set; }

        public ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
    }
}