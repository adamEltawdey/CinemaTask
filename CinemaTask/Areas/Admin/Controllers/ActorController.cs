using CinemaTask.Data;
using CinemaTask.Models;
using CinemaTask.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Threading;

namespace CinemaTask.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ActorController : Controller
    {
        private readonly IRepository<Actor> _ActorRepository;
        //private readonly ApplicationDbContext _context;
        //private readonly IWebHostEnvironment _env;

        public  ActorController(IRepository<Actor> ActorRepository)
        {
            _ActorRepository = ActorRepository;
        }


        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var actors = await _ActorRepository.GetAsync(tracked: false, cancellationToken: cancellationToken);

            // Add Filter

            return View(actors.AsEnumerable());
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Actor());
        }
        [HttpPost]
        public async Task<IActionResult> Create(Actor actor, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(actor);
            }

            await _ActorRepository.AddAsync(actor, cancellationToken);
            await _ActorRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var actot = await _ActorRepository.GetOneAsync(e => e.ActorId == id, cancellationToken: cancellationToken);
            if (actot is null)
                return RedirectToAction("NotFoundPage", "Home");
            return View(actot);

        }

        public async Task<IActionResult> Edit(Actor actor, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                //ModelState.AddModelError(string.Empty, "Any More Errors");

                return View(actor);
            }

            _ActorRepository.Update(actor);
            await _ActorRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var category = await _ActorRepository.GetOneAsync(e => e.ActorId == id, cancellationToken: cancellationToken);

            if (category is null)
                return RedirectToAction("NotFoundPage", "Home");

            _ActorRepository.Delete(category);
            await _ActorRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }
    }


}
