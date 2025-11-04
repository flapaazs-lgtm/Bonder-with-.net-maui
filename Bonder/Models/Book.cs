using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.Models
{
    public class Book
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public List<string> Authors { get; set; } = new();
        public string Description { get; set; }
        public List<string> Genres { get; set; } = new();
        public string CoverUrl { get; set; }
        public int? FirstPublishYear { get; set; }
        public double? Rating { get; set; }
        public string OpenLibraryKey { get; set; }
    }
}
