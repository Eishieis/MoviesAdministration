using Microsoft.EntityFrameworkCore;
using MoviesAdministration.Models;

namespace MoviesAdministration.Data
{
    public class MovieDbContext : DbContext
    {
        public MovieDbContext(DbContextOptions<MovieDbContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }

        public DbSet<Genre> Genres { get; set; }

        public DbSet<AudienceRating> AudienceRatings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Movie>()
                .HasOne(m => m.Genre)
                .WithMany(g => g.Movies)
                .HasForeignKey(m => m.GenreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Movie>()
                .HasOne(m => m.AudienceRating)
                .WithMany(a => a.Movies)
                .HasForeignKey(m => m.AudienceRatingId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}