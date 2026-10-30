using System.ComponentModel.DataAnnotations;

namespace MoviesAdministration.Models
{
    public class AudienceRating
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}