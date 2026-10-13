using System.ComponentModel.DataAnnotations;

namespace MoviesAdministration.Models
{
    public class CriticReview
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        public bool IsPublished { get; set; }

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }
    }
}