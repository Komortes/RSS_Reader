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
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;
using System.Security.Policy;

namespace RSS_Reader.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RssDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly RssReaderService _rssReaderService;

        public HomeController(ILogger<HomeController> logger, RssDbContext context, HttpClient httpClient, RssReaderService rssReaderService)
        {
            _logger = logger;
            _context = context;
            _httpClient = httpClient;
            _rssReaderService = rssReaderService;
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

        public async Task<IActionResult> FeedDetail(int id)
        {
            var feed = _context.Feeds.Include(f => f.Articles).FirstOrDefault(f => f.Id == id);

            if (feed == null)
            {
                return NotFound();
            }

            if (feed.Articles == null || feed.Articles.Count() == 0)
            {
                try
                {
                    await _rssReaderService.UpdateFeedArticles(feed);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    return StatusCode(500, "There was an error loading the articles. Please try again.");
                }
            }

            return View(feed);
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
                    var syndicationFeed = SyndicationFeed.Load(reader);

                    if (syndicationFeed == null)
                    {
                        return Json(new { error = "Unable to parse the RSS Feed." });
                    }

                    var newFeed = new FeedModel
                    {
                        Name = name,
                        Url = url,
                        Description = syndicationFeed.Description.Text,
                        ImageUrl = syndicationFeed.ImageUrl?.AbsoluteUri,
                        LastUpdated = syndicationFeed.LastUpdatedTime.DateTime,
                        Articles = new List<ArticleModel>()
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

        [HttpGet]
        public async Task<IActionResult> RefreshArticles(int feedId)
        {
            var feed = _context.Feeds.Find(feedId);

            if (feed == null)
            {
                return NotFound();
            }

            try
            {
                var oldArticles = await _context.Articles.Where(a => a.RssFeedId == feedId).ToListAsync();
                _context.Articles.RemoveRange(oldArticles);
                await _context.SaveChangesAsync();
                await _rssReaderService.UpdateFeedArticles(feed);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There was an error refreshing the articles. Please try again.");
            }


            return PartialView("~/Views/Components/ArticleCard.cshtml", feed.Articles);
        }

        [HttpGet]
        public IActionResult FilterAndSearchArticles(string searchText, DateTime? startDate, DateTime? endDate, int feedId)
        {
            var feed = _context.Feeds.Include(f => f.Articles).FirstOrDefault(f => f.Id == feedId);

            if (feed == null)
            {
                return NotFound();
            }

            if (feed.Articles == null)
            {
                return NotFound();
            }

            var filteredArticles = feed.Articles.AsQueryable();

            if (startDate.HasValue && endDate.HasValue)
            {
                filteredArticles = filteredArticles
                    .Where(a => a.PublishDate.Date >= startDate.Value.Date && a.PublishDate.Date <= endDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                filteredArticles = filteredArticles
                    .Where(a => a.Title != null && a.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            var finalResult = filteredArticles.ToList();

            if (finalResult.Count == 0)
            {
                return NotFound("No articles found.");
            }

            return PartialView("~/Views/Components/ArticleCard.cshtml", finalResult);
        }






    }
}
