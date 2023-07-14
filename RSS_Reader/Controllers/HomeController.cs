using Microsoft.AspNetCore.Mvc;
using RSS_Reader.Data;
using RSS_Reader.Models;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.ServiceModel.Syndication;
using System.Xml;
using RSS_Reader.Models.Domain;
using System.Threading.Tasks;
using System.Net.Http;

namespace RSS_Reader.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RssDbContext _context;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger, RssDbContext context, HttpClient httpClient)
        {
            _logger = logger;
            _context = context;
            _httpClient = httpClient;
        }

        public IActionResult Index()
        {
            var feeds = _context.Feeds.ToList();
            return View(feeds);
        }

        [HttpGet]
        public IActionResult GetFeeds()
        {
            var feeds = _context.Feeds.ToList();
            return PartialView("Index", feeds);
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
                return Json(new { error = "Both URL and Name must be provided." });
            }

            try
            {
                using (var reader = XmlReader.Create(await _httpClient.GetStreamAsync(url)))
                {
                    var feed = SyndicationFeed.Load(reader);

                    if (feed == null)
                    {
                        return Json(new { error = "Unable to parse the RSS Feed." });
                    }

                    var newFeed = new FeedModel
                    {
                        Name = name,
                        Url = url,
                        Description = feed.Description.Text,
                        ImageUrl = feed.ImageUrl?.AbsoluteUri,
                        LastUpdated = feed.LastUpdatedTime.DateTime
                    };

                    _context.Feeds.Add(newFeed);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error adding feed with URL {url}");
                return Json(new { error = "There was an error adding the feed. Please try again." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFeeds([FromBody] List<int> feedIds)
        {
            try
            {
                var feedsToDelete = _context.Feeds.Where(feed => feedIds.Contains(feed.Id));

                if (feedsToDelete == null || !feedsToDelete.Any())
                {
                    return Json(new { error = "No feeds found to delete." });
                }

                _context.Feeds.RemoveRange(feedsToDelete);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error deleting feeds with IDs {string.Join(", ", feedIds)}");
                return Json(new { error = "There was an error deleting the feeds. Please try again." });
            }
        }

    }
}
