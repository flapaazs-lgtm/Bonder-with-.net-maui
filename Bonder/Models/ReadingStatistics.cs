using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.Models
{
    class ReadingStatistics
    {
        public int BooksCurrentlyReading { get; set; }
        public int BooksFinished { get; set; }
        public int BooksLiked { get; set; }
        public int BooksFinishedThisYear { get; set; }
        public int TotalPagesRead { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public List<string> FavoriteGenres { get; set; } = new();
        public List<string> FavoriteAuthors { get; set; } = new();
        public double AverageRating { get; set; }
        public Dictionary<int, int> BooksPerMonth { get; set; } = new();
        public Dictionary<string, int> GenreDistribution { get; set; } = new();
    }
}
