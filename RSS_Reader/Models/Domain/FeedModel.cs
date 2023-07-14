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
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [Url]
        [StringLength(500)]
        public string Url { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Url]
        [StringLength(500)]
        public string ImageUrl { get; set; }

        public DateTime LastUpdated { get; set; }  

        public virtual ICollection<ArticleModel> Articles { get; set; }
    }
}
