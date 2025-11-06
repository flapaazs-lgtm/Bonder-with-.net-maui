using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.Models
{
   public class ReadingBook : Book
    {
        public double Progress { get; set; }
        public DateTime StartedDate { get; set; }
        public DateTime? LastReadDate { get; set; }
    }
}
