using Bonder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppTheme = Bonder.Models.AppTheme;

namespace Bonder.Services
{
    public interface IThemeService
    {
        AppTheme CurrentTheme { get; }
        Task SetThemeAsync(AppTheme theme);
        Task<ThemeSettings> GetThemeSettingsAsync();
        Task SaveThemeSettingsAsync(ThemeSettings settings);
        void ApplyTheme(AppTheme theme);
    }
}
