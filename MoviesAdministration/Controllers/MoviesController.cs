using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MoviesAdministration.Data;
using MoviesAdministration.Models;
using MoviesAdministration.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MoviesAdministration.Controllers
{
    public class MoviesController : Controller
    {
        private readonly MovieDbContext _context;
        private readonly BlobStorageService _blobStorage;

        private const long MaxImageSize = 5 * 1024 * 1024;

        private static readonly string[] AllowedExtensions =
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp"
        };

        private static readonly string[] AllowedContentTypes =
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp"
        };

        public MoviesController(
            MovieDbContext context,
            BlobStorageService blobStorage)
        {
            _context = context;
            _blobStorage = blobStorage;
        }

        private async Task LoadMovieOptionsAsync()
        {
            ViewBag.Genres = await _context.Genres
                .OrderBy(g => g.Name)
                .ToListAsync();

            ViewBag.AudienceRatings = await _context.AudienceRatings
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        private async Task ValidateRelationships(Movie movie)
        {
            if (!await _context.Genres.AnyAsync(g => g.Id == movie.GenreId))
            {
                ModelState.AddModelError(
                    "GenreId",
                    "Please select a valid genre.");
            }

            if (!await _context.AudienceRatings.AnyAsync(
                a => a.Id == movie.AudienceRatingId))
            {
                ModelState.AddModelError(
                    "AudienceRatingId",
                    "Please select a valid audience rating.");
            }
        }

        public async Task<IActionResult> Index(bool openCreate = false)
        {
            var movies = await _context.Movies
                .Include(m => m.Genre)
                .Include(m => m.AudienceRating)
                .OrderByDescending(m => m.ReleaseDate)
                .ToListAsync();

            ViewBag.OpenCreate = openCreate;

            return View(movies);
        }

        public async Task<IActionResult> DetailsModal(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.Genre)
                .Include(m => m.AudienceRating)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            return PartialView("moviedetails", movie);
        }

        public async Task<IActionResult> Image(
            string? blobName,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(blobName) ||
                blobName.Contains('/') ||
                blobName.Contains('\\'))
            {
                return BadRequest();
            }

            var image = await _blobStorage.DownloadAsync(
                blobName,
                cancellationToken);

            if (image == null)
            {
                return NotFound();
            }

            return File(
                image.Content,
                image.Details.ContentType ?? "application/octet-stream");
        }

        public async Task<IActionResult> Create()
        {
            await LoadMovieOptionsAsync();

            return PartialView("moviecreate", new Movie
            {
                ReleaseDate = DateTime.Today
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Movie movie,
            IFormFile? imageFile,
            CancellationToken cancellationToken)
        {
            ValidateImage(imageFile);

            await ValidateRelationships(movie);

            if (!ModelState.IsValid)
            {
                await LoadMovieOptionsAsync();
                return PartialView("moviecreate", movie);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    movie.ImageBlobName = await _blobStorage.UploadAsync(
                        imageFile,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(
                        "imageFile",
                        $"Upload error: {ex.GetType().Name} - {ex.Message}");

                    await LoadMovieOptionsAsync();
                    return PartialView("moviecreate", movie);
                }
            }

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
            {
                return NotFound();
            }

            await LoadMovieOptionsAsync();

            return PartialView("movieedit", movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Movie movie,
            IFormFile? imageFile,
            CancellationToken cancellationToken)
        {
            if (id != movie.Id)
            {
                return BadRequest();
            }

            var existingMovie = await _context.Movies.FindAsync(id);

            if (existingMovie == null)
            {
                return NotFound();
            }

            ValidateImage(imageFile);

            await ValidateRelationships(movie);

            if (!ModelState.IsValid)
            {
                movie.ImageBlobName = existingMovie.ImageBlobName;

                await LoadMovieOptionsAsync();

                return PartialView("movieedit", movie);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    existingMovie.ImageBlobName =
                        await _blobStorage.UploadAsync(
                            imageFile,
                            cancellationToken);
                }
                catch (Exception ex)
                {
                    movie.ImageBlobName = existingMovie.ImageBlobName;

                    ModelState.AddModelError(
                        "imageFile",
                        $"Upload error: {ex.GetType().Name} - {ex.Message}");

                    await LoadMovieOptionsAsync();

                    return PartialView("movieedit", movie);
                }
            }

            existingMovie.Title = movie.Title;
            existingMovie.Synopsis = movie.Synopsis;
            existingMovie.GenreId = movie.GenreId;
            existingMovie.AudienceRatingId = movie.AudienceRatingId;
            existingMovie.RuntimeHours = movie.RuntimeHours;
            existingMovie.RuntimeMinutes = movie.RuntimeMinutes;
            existingMovie.ReleaseDate = movie.ReleaseDate;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
            {
                return NotFound();
            }

            return PartialView("moviedelete", movie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
            {
                return NotFound();
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        private bool ValidateImage(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return true;
            }

            if (imageFile.Length > MaxImageSize)
            {
                ModelState.AddModelError(
                    "imageFile",
                    "The image must be 5 MB or smaller.");

                return false;
            }

            string extension =
                Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension) ||
                !AllowedContentTypes.Contains(imageFile.ContentType))
            {
                ModelState.AddModelError(
                    "imageFile",
                    "Upload a JPG, PNG, GIF, or WebP image.");

                return false;
            }

            return true;
        }
    }
}