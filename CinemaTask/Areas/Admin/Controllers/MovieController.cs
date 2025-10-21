using CinemaTask.Data;
using CinemaTask.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CinemaTask.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public MovieController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            var Movies = _context.Movies.ToList();
            return View(Movies);
        }
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Categories"] = _context.Categories.ToList();
            ViewData["Cinemas"] = _context.Cinemas.ToList();
            ViewData["Actors"] = _context.Actors.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Movie movie, IFormFile MainImage, List<IFormFile> SubImages)
        {
            if (!ModelState.IsValid)
                return View(movie);

            // handle main image
            if (MainImage != null && MainImage.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(MainImage.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "movies", fileName);

                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using var stream = new FileStream(path, FileMode.Create);
                MainImage.CopyTo(stream);

                movie.MainImage = fileName;
            }

            _context.Movies.Add(movie);
            _context.SaveChanges();

            // handle sub images
            if (SubImages != null && SubImages.Count > 0)
            {
                foreach (var img in SubImages)
                {
                    if (img.Length > 0)
                    {
                        var subFileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                        var subPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "movies", subFileName);

                        using var stream = new FileStream(subPath, FileMode.Create);
                        img.CopyTo(stream);

                        movie.SubImage = subFileName; // optional: for single string property
                    }
                }

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var movie = _context.Movies.Find(id);
            if (movie == null)
                return NotFound();

            ViewData["Categories"] = _context.Categories.ToList();
            ViewData["Cinemas"] = _context.Cinemas.ToList();
            ViewData["Actors"] = _context.Actors.ToList();
            return View(movie);
        }

        [HttpPost]
        public IActionResult Edit(Movie movie, IFormFile? MainImage)
        {
            var existing = _context.Movies.Find(movie.Id);
            if (existing == null)
                return NotFound();

            existing.Name = movie.Name;
            existing.Description = movie.Description;
            existing.Price = movie.Price;
            existing.Status = movie.Status;
            existing.CategoryId = movie.CategoryId;
            existing.CinemaId = movie.CinemaId;
            existing.ActorId = movie.ActorId;
            existing.DateTime = movie.DateTime;

            if (MainImage != null && MainImage.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(MainImage.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "movies", fileName);

                using var stream = new FileStream(path, FileMode.Create);
                MainImage.CopyTo(stream);

                existing.MainImage = fileName;
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var movie = _context.Movies.Find(id);
            if (movie == null)
                return NotFound();

            _context.Movies.Remove(movie);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
