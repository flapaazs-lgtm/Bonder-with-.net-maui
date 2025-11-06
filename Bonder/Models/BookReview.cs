using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.Models
{
    class BookReview
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string BookId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public double Rating { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public List<string> Tags { get; set; } = new();
        public int LikesCount { get; set; }
        public bool IsSpoiler { get; set; }
    }
}
