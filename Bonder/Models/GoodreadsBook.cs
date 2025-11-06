using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.Models
{
    class GoodreadsBook
    {
        public string GoodreadsId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public double AverageRating { get; set; }
        public int RatingsCount { get; set; }
        public int ReviewsCount { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public List<string> Shelves { get; set; } = new();
    }
}
