using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.Models
{
    class ReadingChallenge
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Description { get; set; }
        public int TargetCount { get; set; }
        public int CurrentProgress { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ChallengeType Type { get; set; }
        public string IconEmoji { get; set; }
        public bool IsCompleted => CurrentProgress >= TargetCount;
        public double ProgressPercentage => TargetCount > 0 ? (double)CurrentProgress / TargetCount : 0;
    }

    public enum ChallengeType
    {
        BookCount,
        PagesRead,
        GenreExploration,
        AuthorDiversity,
        Custom
    }
}
