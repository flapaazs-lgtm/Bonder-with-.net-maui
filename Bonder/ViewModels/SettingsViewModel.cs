using Bonder.Models;
using Bonder.Services;
using AppTheme = Bonder.Models.AppTheme;

namespace Bonder.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly IThemeService _themeService;
    private readonly IAuthenticationService _authService;
    private ThemeSettings _themeSettings;

    public ThemeSettings ThemeSettings
    {
        get => _themeSettings;
        set => SetProperty(ref _themeSettings, value);
    }

    public bool IsDarkMode
    {
        get => ThemeSettings?.Theme == AppTheme.Dark;
        set
        {
            if (ThemeSettings != null)
            {
                ThemeSettings.Theme = value ? AppTheme.Dark : AppTheme.Light;
                ThemeSettings.UseSystemTheme = false;
                _ = SaveThemeAsync();
            }
        }
    }

    public bool UseSystemTheme
    {
        get => ThemeSettings?.UseSystemTheme ?? true;
        set
        {
            if (ThemeSettings != null)
            {
                ThemeSettings.UseSystemTheme = value;
                _ = SaveThemeAsync();
            }
        }
    }

    public Command ToggleDarkModeCommand { get; }
    public Command ToggleSystemThemeCommand { get; }
    public Command LogOutCommand { get; }
    public Command BackCommand { get; }

    public SettingsViewModel(IThemeService themeService, IAuthenticationService authService)
    {
        _themeService = themeService;
        _authService = authService;

        ToggleDarkModeCommand = new Command(() => IsDarkMode = !IsDarkMode);
        ToggleSystemThemeCommand = new Command(() => UseSystemTheme = !UseSystemTheme);
        LogOutCommand = new Command(async () => await LogOutAsync());
        BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

        _ = LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        ThemeSettings = await _themeService.GetThemeSettingsAsync();
        OnPropertyChanged(nameof(IsDarkMode));
        OnPropertyChanged(nameof(UseSystemTheme));
    }

    private async Task SaveThemeAsync()
    {
        await _themeService.SaveThemeSettingsAsync(ThemeSettings);
        OnPropertyChanged(nameof(IsDarkMode));
        OnPropertyChanged(nameof(UseSystemTheme));
    }

    private async Task LogOutAsync()
    {
        var confirm = await Shell.Current.DisplayAlert(
            "Log Out",
            "Are you sure you want to log out?",
            "Yes",
            "No");

        if (confirm)
        {
            await _authService.SignOutAsync();
            await Shell.Current.GoToAsync("//SignIn");
        }
    }
}