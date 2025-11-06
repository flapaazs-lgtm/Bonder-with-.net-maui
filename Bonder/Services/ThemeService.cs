using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bonder.Services;

namespace Bonder.Models
{
    class ThemeService 
    {
        private const string ThemeSettingsKey = "theme_settings";
        private ThemeSettings _currentSettings;

        public AppTheme CurrentTheme => _currentSettings?.Theme ?? AppTheme.Light;

        public ThemeService()
        {
            _ = LoadThemeSettingsAsync();
        }

        public async Task<ThemeSettings> GetThemeSettingsAsync()
        {
            if (_currentSettings != null)
                return _currentSettings;

            await LoadThemeSettingsAsync();
            return _currentSettings;
        }

        public async Task SaveThemeSettingsAsync(ThemeSettings settings)
        {
            _currentSettings = settings;

            var json = System.Text.Json.JsonSerializer.Serialize(settings);
            Preferences.Default.Set(ThemeSettingsKey, json);

            if (settings.UseSystemTheme)
            {
                ApplyTheme(AppTheme.System);
            }
            else
            {
                ApplyTheme(settings.Theme);
            }

            await Task.CompletedTask;
        }

        public async Task SetThemeAsync(AppTheme theme)
        {
            var settings = await GetThemeSettingsAsync();
            settings.Theme = theme;
            settings.UseSystemTheme = theme == AppTheme.System;

            await SaveThemeSettingsAsync(settings);
        }

        public void ApplyTheme(AppTheme theme)
        {
            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;

            // Remove existing theme dictionaries
            var existingTheme = mergedDictionaries.FirstOrDefault(d =>
                d.Source?.OriginalString?.Contains("DarkTheme") == true ||
                d.Source?.OriginalString?.Contains("LightTheme") == true);

            if (existingTheme != null)
                mergedDictionaries.Remove(existingTheme);

            // Determine which theme to apply
            var themeToApply = theme;
            if (theme == AppTheme.System)
            {
                themeToApply = Application.Current.RequestedTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark
                    ? AppTheme.Dark
                    : AppTheme.Light;
            }

            // Add new theme dictionary
            var themeUri = themeToApply == AppTheme.Dark
                ? "Resources/Styles/DarkTheme.xaml"
                : "Resources/Styles/LightTheme.xaml";

            mergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri(themeUri, UriKind.Relative)
            });
        }

        private async Task LoadThemeSettingsAsync()
        {
            await Task.CompletedTask;

            var json = Preferences.Default.Get(ThemeSettingsKey, string.Empty);

            if (string.IsNullOrEmpty(json))
            {
                _currentSettings = new ThemeSettings
                {
                    Theme = AppTheme.Light,
                    UseSystemTheme = true
                };
            }
            else
            {
                try
                {
                    _currentSettings = System.Text.Json.JsonSerializer.Deserialize<ThemeSettings>(json);
                }
                catch
                {
                    _currentSettings = new ThemeSettings();
                }
            }

            ApplyTheme(_currentSettings.UseSystemTheme ? AppTheme.System : _currentSettings.Theme);
        }
    }
}
