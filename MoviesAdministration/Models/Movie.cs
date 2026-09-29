using System.ComponentModel.DataAnnotations;

namespace MoviesAdministration.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Synopsis { get; set; }

        [Required]
        public string Genre { get; set; }

        [Required]
        public string Rating { get; set; }

        [Range(0, 10)]
        public int RuntimeHours { get; set; }

        [Range(0, 59)]
        public int RuntimeMinutes { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }
    }
}