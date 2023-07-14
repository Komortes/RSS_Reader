using Microsoft.AspNetCore.Mvc;
using RSS_Reader.Data;
using RSS_Reader.Models;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.ServiceModel.Syndication;
using System.Xml;
using RSS_Reader.Models.Domain;

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

        [HttpPost]
        public async Task<IActionResult> CreateFeed(string url, string name)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(name))
            {
                return BadRequest("Both URL and Name must be provided.");
            }

            try
            {
                using (var client = new HttpClient())
                using (var reader = XmlReader.Create(await client.GetStreamAsync(url)))
                {
                    var feed = SyndicationFeed.Load(reader);

                    if (feed == null)
                    {
                        return BadRequest("Unable to parse the RSS Feed.");
                    }

                    var newFeed = new FeedModel
                    {
                        Name = name,
                        Url = url,
                        LastUpdated = feed.LastUpdatedTime.DateTime
                    };

                    _context.Feeds.Add(newFeed);
                    await _context.SaveChangesAsync();

                    return Ok("Feed added successfully");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error adding feed with URL {url}");
                return StatusCode(500, "There was an error adding the feed. Please try again.");
            }
        }
    }
}