using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.Models
{
    public class ThemeSettings
    {
        public AppTheme Theme { get; set; } = AppTheme.Light;
        public bool UseSystemTheme { get; set; } = true;
        public string AccentColor { get; set; } = "#D4A574";
    }

    public enum AppTheme
    {
        Light,
        Dark,
        System
    }
}
