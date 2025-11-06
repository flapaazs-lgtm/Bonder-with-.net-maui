using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.Models
{
    class ReadingSession
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string BookId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int PagesRead { get; set; }
        public double ProgressBefore { get; set; }
        public double ProgressAfter { get; set; }
        public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : TimeSpan.Zero;
    }
}
