using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RSS_Reader.Models.Domain
{
    public class FeedModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(250)]
        public string Name { get; set; }

        [Required]
        [Url]
        [StringLength(2000)]
        public string Url { get; set; }

        [StringLength(4000)]
        public string Description { get; set; }

        [Url]
        [StringLength(2000)]
        public string ImageUrl { get; set; }

        public DateTime LastUpdated { get; set; }  

        public virtual ICollection<ArticleModel> Articles { get; set; }
    }
}
