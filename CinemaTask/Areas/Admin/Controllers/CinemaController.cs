using CinemaTask.Models;
using CinemaTask.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace CinemaTask.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CinemaController : Controller
    {
        private readonly IRepository<Cinema> _cinemaRepository;

        public CinemaController(IRepository<Cinema> cinemaRepository)
        {
            _cinemaRepository = cinemaRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var cinemas = await _cinemaRepository.GetAsync(tracked: false, cancellationToken: cancellationToken);
            return View(cinemas.AsEnumerable());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Cinema());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Cinema cinema, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(cinema);

            await _cinemaRepository.AddAsync(cinema, cancellationToken);
            await _cinemaRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var cinema = await _cinemaRepository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);
            if (cinema == null)
                return RedirectToAction("NotFoundPage", "Home");

            return View(cinema);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Cinema cinema, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(cinema);

            _cinemaRepository.Update(cinema);
            await _cinemaRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var cinema = await _cinemaRepository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);
            if (cinema == null)
                return RedirectToAction("NotFoundPage", "Home");

            _cinemaRepository.Delete(cinema);
            await _cinemaRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }
    }
}
