using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.Models
{
    class BarcodeScanResult
    {
        public string ISBN { get; set; }
        public string Format { get; set; }
        public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
    }
}
