using CinemaTask.Data;
using CinemaTask.Migrations;
using CinemaTask.Models;
using CinemaTask.Repositories.IRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace CinemaTask.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MovieController : Controller
    {
        private readonly IRepository<Movie> _MovieRepository;
        //private readonly ApplicationDbContext _context;
        //private readonly IWebHostEnvironment _env;

        public MovieController(IRepository<Movie> MovieRepository)
        {
            _MovieRepository = MovieRepository;
        }


        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var movies = await _MovieRepository.GetAsync(tracked: false, cancellationToken: cancellationToken);

            // Add Filter

            return View(movies.AsEnumerable());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Movie());
        }


        [HttpPost]
        public async Task<IActionResult> Create(Movie movie, IFormFile? MainImageFile, IFormFile? SubImageFile, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(movie);
            }

            // Handle image uploads
            if (MainImageFile != null && MainImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(MainImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/movies", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                using var stream = new FileStream(filePath, FileMode.Create);
                await MainImageFile.CopyToAsync(stream, cancellationToken);
                movie.MainImage = "/images/movies/" + fileName;
            }

            if (SubImageFile != null && SubImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(SubImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/movies", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                using var stream = new FileStream(filePath, FileMode.Create);
                await SubImageFile.CopyToAsync(stream, cancellationToken);
                movie.SubImage = "/images/movies/" + fileName;
            }

            await _MovieRepository.AddAsync(movie, cancellationToken);
            await _MovieRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public IActionResult Edit()
        {
            return View(new Movie());
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Movie movie, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(movie);
            }
            _MovieRepository.Update(movie);
            await _MovieRepository.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var category = await _MovieRepository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);

            if (category is null)
                return RedirectToAction("NotFoundPage", "Home");

            _MovieRepository.Delete(category);
            await _MovieRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }
    }
}