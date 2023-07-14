using Microsoft.AspNetCore.Mvc;
using RSS_Reader.Data;
using RSS_Reader.Models;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace RSS_Reader.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RssDbContext _context;

        public HomeController(ILogger<HomeController> logger, RssDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var feeds = _context.Feeds.ToList();
            return View(feeds);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}