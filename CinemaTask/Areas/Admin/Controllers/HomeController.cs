using Microsoft.AspNetCore.Mvc;
using CinemaTask.Models;


namespace CinemaTask.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
