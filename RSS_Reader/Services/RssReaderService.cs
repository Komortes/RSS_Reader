using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using RSS_Reader.Models.Domain;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using HtmlAgilityPack;

public class RssReaderService
{
    private readonly ILogger<RssReaderService> _logger;

    public RssReaderService(ILogger<RssReaderService> logger)
    {
        _logger = logger;
    }

    public async Task UpdateFeedArticles(FeedModel feed)
    {
        try
        {
            using (var client = new HttpClient())
            using (var reader = XmlReader.Create(await client.GetStreamAsync(feed.Url)))
            {
                var syndicationFeed = SyndicationFeed.Load(reader);

                if (syndicationFeed == null)
                {
                    throw new Exception("Unable to parse the RSS Feed.");
                }

                foreach (var item in syndicationFeed.Items)
                {
                    var newArticle = new ArticleModel
                    {
                        Title = item.Title.Text,
                        Link = item.Links[0].Uri.ToString(),
                        Description = item.Summary != null ? StripHtmlTags(item.Summary.Text) : "No description available",
                        PublishDate = item.PublishDate.DateTime
                    };

                    if (feed.Articles == null)
                    {
                        feed.Articles = new List<ArticleModel>();
                    }
                    feed.Articles.Add(newArticle);
                }

            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error loading articles for feed with URL {feed.Url}");
            throw;
        }
    }

    public SyndicationFeed ReadFeed(string url)
    {
        using (var reader = XmlReader.Create(url))
        {
            var feed = SyndicationFeed.Load(reader);
            return feed;
        }
    }

    private static string StripHtmlTags(string html)
    {
        if (string.IsNullOrEmpty(html))
        {
            return html;
        }

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);
        return HtmlEntity.DeEntitize(htmlDoc.DocumentNode.InnerText);
    }
}
