using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.Models
{
    class SearchFilter
    {
        public string Query { get; set; }
        public List<string> Genres { get; set; } = new();
        public double? MinRating { get; set; }
        public double? MaxRating { get; set; }
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public int? MinPages { get; set; }
        public int? MaxPages { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public string Language { get; set; }
        public SortOption SortBy { get; set; } = SortOption.Relevance;
    }

    public enum SortOption
    {
        Relevance,
        Title,
        Author,
        Rating,
        PublicationDate,
        Popularity
    }
}
