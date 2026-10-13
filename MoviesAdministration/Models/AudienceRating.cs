using System.ComponentModel.DataAnnotations;

namespace MoviesAdministration.Models
{
    public class AudienceRating
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;
    }
}