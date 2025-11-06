using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.Models
{
    public class UserPreferences
    {
        public Dictionary<string, double> GenreWeights { get; set; } = new();
        public Dictionary<string, double> AuthorWeights { get; set; } = new();
        public List<string> LikedBookIds { get; set; } = new();
        public List<string> DislikedBookIds { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
