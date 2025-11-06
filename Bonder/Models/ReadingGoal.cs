using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.Models
{
    class ReadingGoal
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int Year { get; set; } = DateTime.UtcNow.Year;
        public int TargetBooks { get; set; }
        public int BooksRead { get; set; }
        public bool IsCompleted => BooksRead >= TargetBooks;
        public double Progress => TargetBooks > 0 ? (double)BooksRead / TargetBooks : 0;
    }
}
