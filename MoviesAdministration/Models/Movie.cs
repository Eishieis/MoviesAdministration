using System;
using System.ComponentModel.DataAnnotations;

namespace MoviesAdministration.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Synopsis { get; set; } = string.Empty;

        public int GenreId { get; set; }

        public Genre? Genre { get; set; }

        public int AudienceRatingId { get; set; }

        public AudienceRating? AudienceRating { get; set; }

        [Range(0, 10)]
        public int RuntimeHours { get; set; }

        [Range(0, 59)]
        public int RuntimeMinutes { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        public string? ImageBlobName { get; set; }
    }
}