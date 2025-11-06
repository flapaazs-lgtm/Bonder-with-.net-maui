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
        builder.Services.AddSingleton<IStorageService, StorageService>();
        builder.Services.AddSingleton<IRecommendationService, RecommendationService>();

        // Register ViewModels
        builder.Services.AddTransient<SignInViewModel>();
        builder.Services.AddTransient<GenreSelectionViewModel>();
        builder.Services.AddTransient<TopBooksSelectionViewModel>();
        builder.Services.AddTransient<SwipeDiscoveryViewModel>();
        builder.Services.AddTransient<BookDetailsViewModel>();
        builder.Services.AddTransient<LibraryViewModel>();
        builder.Services.AddTransient<MainViewModel>();

        // Register Views
        builder.Services.AddTransient<SignInPage>();
        builder.Services.AddTransient<GenreSelectionPage>();
        builder.Services.AddTransient<TopBooksSelectionPage>();
        builder.Services.AddTransient<SwipeDiscoveryPage>();
        builder.Services.AddTransient<BookDetailsPage>();
        builder.Services.AddTransient<LibraryPage>();
        builder.Services.AddTransient<MainPage>();

        // Register Converters
        builder.Services.AddSingleton<GenreSelectionConverter>();
        builder.Services.AddSingleton<GenreTextColorConverter>();
        builder.Services.AddSingleton<InverseBoolConverter>();
        builder.Services.AddSingleton<SelectedBookBorderConverter>();
        builder.Services.AddSingleton<TabColorConverter>();
        builder.Services.AddSingleton<TabFontConverter>();

        return builder.Build();
    }
}