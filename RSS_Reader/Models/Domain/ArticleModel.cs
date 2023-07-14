using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RSS_Reader.Models.Domain
{
    public class ArticleModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Url]
        [StringLength(1000)]
        public string Link { get; set; }

        [Required]
        public DateTime PublishDate { get; set; }

        [Required]
        public int RssFeedId { get; set; }

        [ForeignKey("RssFeedId")]
        public virtual FeedModel RssFeed { get; set; }
    }
}
