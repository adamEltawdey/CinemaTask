using CinemaTask.Data;
using CinemaTask.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace CinemaTask.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ActorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ActorController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            var actors = _context.Actors.ToList();
            return View(actors);
        }

        public IActionResult Create()
        {
            return View(new Actor());
        }

        [HttpPost]
        public IActionResult Create(Actor actor, IFormFile ImageFile)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                var path = Path.Combine(_env.WebRootPath, "images/actors", fileName);

                using var stream = new FileStream(path, FileMode.Create);
                ImageFile.CopyTo(stream);

                actor.ActorImage = fileName;
            }

            _context.Actors.Add(actor);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var actor = _context.Actors.Find(id);
            if (actor == null) return NotFound();
            return View(actor);
        }

        [HttpPost]
        public IActionResult Edit(Actor actor, IFormFile? ImageFile)
        {
            var existing = _context.Actors.Find(actor.ActorId);
            if (existing == null) return NotFound();

            existing.ActorName = actor.ActorName;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                var path = Path.Combine(_env.WebRootPath, "images/actors", fileName);

                using var stream = new FileStream(path, FileMode.Create);
                ImageFile.CopyTo(stream);

                existing.ActorImage = fileName;
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
            public IActionResult Delete(int id)
        {
            var actor = _context.Actors.FirstOrDefault(a => a.ActorId == id);
            if (actor == null) return NotFound();

            // delete image if exists
            if (!string.IsNullOrEmpty(actor.ActorImage))
            {
                var path = Path.Combine(_env.WebRootPath, "images", "actors", actor.ActorImage);
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

            _context.Actors.Remove(actor);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }


}
