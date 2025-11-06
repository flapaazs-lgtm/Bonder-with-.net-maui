using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.Models
{
    class BookClub
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Description { get; set; }
        public string CoverImageUrl { get; set; }
        public List<string> MemberIds { get; set; } = new();
        public string CurrentBookId { get; set; }
        public DateTime? NextMeetingDate { get; set; }
        public List<BookClubMessage> Messages { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsPrivate { get; set; }
    }
}
