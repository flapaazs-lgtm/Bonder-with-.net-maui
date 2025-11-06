using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.Models
{
   public class BookNote
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string BookId { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public int? Page { get; set; }
        public string Chapter { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public NoteType Type { get; set; } = NoteType.General;
        public string Color { get; set; } = "#FFD700";
    }
    public enum NoteType
    {
        General,
        Highlight,
        Quote,
        Thought,
        Question
    }
}
