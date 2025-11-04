using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public List<string> FavoriteGenres { get; set; } = new();
        public List<string> FavoriteBookIds { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
