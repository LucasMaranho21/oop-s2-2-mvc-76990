using System.ComponentModel.DataAnnotations;

namespace FoodSafetyInspectionTracker.Models
{
    public class Premises
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Town { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string RiskRating { get; set; } = "Low";

        public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    }
}