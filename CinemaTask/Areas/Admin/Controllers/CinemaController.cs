using CinemaTask.Data;
using CinemaTask.Migrations;
using CinemaTask.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace CinemaTask.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CinemaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CinemaController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            var Cinemas = _context.Cinemas.ToList();
            return View(Cinemas);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var cinema = _context.Cinemas.Find(id);
            if (cinema == null) return NotFound();
            return View(cinema);
        }
        [HttpPost]
        public IActionResult Edit(Models.Cinema cinema)
        {
            var existingCinema = _context.Cinemas.Find(cinema.Id);
            if (existingCinema == null) return NotFound();

            existingCinema.Name = cinema.Name;
            existingCinema.Description = cinema.Description;
            existingCinema.Address = cinema.Address;
            existingCinema.Phone = cinema.Phone;
            existingCinema.Email = cinema.Email;
            existingCinema.Movies = cinema.Movies;

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Cinema());
        }
        [HttpPost]
        public IActionResult Create(Cinema cinema)
        {
            _context.Cinemas.Add(cinema);
            _context.SaveChanges();

           return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(int id)
        {
            var Cinema = _context.Cinemas.FirstOrDefault(a => a.Id == id);
            if (Cinema == null) return NotFound();

            _context.Cinemas.Remove(Cinema); 
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}
