using Microsoft.EntityFrameworkCore;
using RSS_Reader.Models.Domain;

namespace RSS_Reader.Data
{
    public class RssDbContext : DbContext
    {
        public DbSet<FeedModel> Feeds { get; set; }
        public DbSet<ArticleModel> Articles { get; set; }

        public RssDbContext(DbContextOptions<RssDbContext> options) : base(options)
        {

        }

    }
}
