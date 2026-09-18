using Microsoft.AspNetCore.Mvc;
using MoviesAdministration.Models;
using System.Diagnostics;

namespace MoviesAdministration.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Readme()
        {
            return View();
        }
        public HomeController()
        {
            Console.WriteLine("Constructor called.");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}