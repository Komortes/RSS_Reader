using System.Xml;
using System.ServiceModel.Syndication;

public class RssReaderService
{
    public SyndicationFeed ReadFeed(string url)
    {
        using (var reader = XmlReader.Create(url))
        {
            var feed = SyndicationFeed.Load(reader);
            return feed;
        }
    }
}
