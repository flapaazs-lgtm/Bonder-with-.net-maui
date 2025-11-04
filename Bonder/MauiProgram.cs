using Bonder.ViewModels;
using Bonder.Services;
using Bonder.Converters;
using Bonder.Views;

namespace Bonder;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register Services
        builder.Services.AddSingleton<IBookService, OpenLibraryService>();

        // Register ViewModels
        builder.Services.AddTransient<SignInViewModel>();
        builder.Services.AddTransient<GenreSelectionViewModel>();
        builder.Services.AddTransient<MainViewModel>();

        // Register Views - FIXED TYPO HERE
        builder.Services.AddTransient<SignInPage>();
        builder.Services.AddTransient<GenreSelectionPage>();
        builder.Services.AddTransient<MainPage>();

        // Register Converters
        builder.Services.AddSingleton<GenreSelectionConverter>();
        builder.Services.AddSingleton<GenreTextColorConverter>();

        return builder.Build();
    }
}