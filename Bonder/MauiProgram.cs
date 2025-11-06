using Bonder.Bonder.ViewModels;
using Bonder.Converters;
using Bonder.Models;
using Bonder.Services;
using Bonder.ViewModels;
using Bonder.Views;
using CommunityToolkit.Maui;
using System.Collections.ObjectModel;
using ZXing.Net.Maui;

namespace Bonder;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseBarcodeReader()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register Core Services
        builder.Services.AddSingleton<IBookService, OpenLibraryService>();
        builder.Services.AddSingleton<IStorageService, EnhancedStorageService>();
        builder.Services.AddSingleton<IRecommendationService, RecommendationService>();
        builder.Services.AddSingleton<IAuthenticationService, FirebaseAuthService>();
        builder.Services.AddSingleton<IThemeService, ThemeService>();
        builder.Services.AddSingleton<IBarcodeScannerService, BarcodeScannerService>();

        // Register ViewModels
        builder.Services.AddTransient<SignInViewModel>();
        builder.Services.AddTransient<GenreSelectionViewModel>();
        builder.Services.AddTransient<TopBooksSelectionViewModel>();
        builder.Services.AddTransient<SwipeDiscoveryViewModel>();
        builder.Services.AddTransient<BookDetailsViewModel>();
        builder.Services.AddTransient<LibraryViewModel>();
        builder.Services.AddTransient<EnhancedLibraryViewModel>();
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<StatisticsViewModel>();
        builder.Services.AddTransient<BookNotesViewModel>();
        builder.Services.AddTransient<AdvancedSearchViewModel>();
        builder.Services.AddTransient<SearchFiltersViewModel>();
        builder.Services.AddTransient<BarcodeScannerViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Register Views
        builder.Services.AddTransient<SignInPage>();
        builder.Services.AddTransient<GenreSelectionPage>();
        builder.Services.AddTransient<TopBooksSelectionPage>();
        builder.Services.AddTransient<SwipeDiscoveryPage>();
        builder.Services.AddTransient<BookDetailsPage>();
        builder.Services.AddTransient<LibraryPage>();
        builder.Services.AddTransient<EnhancedLibraryPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<StatisticsPage>();
        builder.Services.AddTransient<BookNotesPage>();
        builder.Services.AddTransient<AdvancedSearchPage>();
        builder.Services.AddTransient<SearchFiltersPage>();
        builder.Services.AddTransient<BarcodeScannerPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<SearchPage>();

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

// Enhanced Library ViewModel
namespace Bonder.ViewModels;

public class EnhancedLibraryViewModel : BaseViewModel
{
    private readonly IStorageService _storageService;
    private ObservableCollection<Book> _currentlyReading = new();
    private ObservableCollection<Book> _likedBooks = new();
    private ObservableCollection<Book> _finishedBooks = new();
    private ObservableCollection<Book> _displayedBooks = new();
    private ReadingStatistics _statistics;
    private ReadingChallenge _activeChallenge;
    private string _selectedTab = "Liked";
    private bool _isRefreshing = false;

    public ObservableCollection<Book> CurrentlyReading
    {
        get => _currentlyReading;
        set => SetProperty(ref _currentlyReading, value);
    }

    public ObservableCollection<Book> LikedBooks
    {
        get => _likedBooks;
        set => SetProperty(ref _likedBooks, value);
    }

    public ObservableCollection<Book> FinishedBooks
    {
        get => _finishedBooks;
        set => SetProperty(ref _finishedBooks, value);
    }

    public ObservableCollection<Book> DisplayedBooks
    {
        get => _displayedBooks;
        set => SetProperty(ref _displayedBooks, value);
    }

    public ReadingStatistics Statistics
    {
        get => _statistics;
        set => SetProperty(ref _statistics, value);
    }

    public ReadingChallenge ActiveChallenge
    {
        get => _activeChallenge;
        set
        {
            SetProperty(ref _activeChallenge, value);
            OnPropertyChanged(nameof(HasActiveChallenge));
            OnPropertyChanged(nameof(ChallengeProgressText));
        }
    }

    public string SelectedTab
    {
        get => _selectedTab;
        set
        {
            SetProperty(ref _selectedTab, value);
            UpdateDisplayedBooks();
        }
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public bool HasActiveChallenge => ActiveChallenge != null;
    public string ChallengeProgressText => ActiveChallenge != null
        ? $"{ActiveChallenge.CurrentProgress} of {ActiveChallenge.TargetCount} books"
        : string.Empty;

    public Command RefreshCommand { get; }
    public Command ViewBookCommand { get; }
    public Command<Book> ContinueReadingCommand { get; }
    public Command<string> SelectTabCommand { get; }
    public Command NavigateToDiscoverCommand { get; }
    public Command NavigateToLibraryCommand { get; }
    public Command NavigateToSearchCommand { get; }
    public Command NavigateToProfileCommand { get; }

    public EnhancedLibraryViewModel(IStorageService storageService)
    {
        _storageService = storageService;

        RefreshCommand = new Command(async () => await RefreshLibraryAsync());
        ViewBookCommand = new Command<Book>(async (book) => await ViewBookAsync(book));
        ContinueReadingCommand = new Command<Book>(async (book) => await ContinueReadingAsync(book));
        SelectTabCommand = new Command<string>(tab => SelectedTab = tab);

        NavigateToDiscoverCommand = new Command(async () => await Shell.Current.GoToAsync("//SwipeDiscovery"));
        NavigateToLibraryCommand = new Command(async () => await Shell.Current.GoToAsync("//Library"));
        NavigateToSearchCommand = new Command(async () => await Shell.Current.GoToAsync("//Search"));
        NavigateToProfileCommand = new Command(async () => await Shell.Current.GoToAsync("//Profile"));

        _ = LoadLibraryAsync();
    }

    private async Task LoadLibraryAsync()
    {
        try
        {
            var currentlyReading = await _storageService.GetCurrentlyReadingAsync();
            var liked = await _storageService.GetLikedBooksAsync();
            var finished = await _storageService.GetFinishedBooksAsync();
            var stats = await _storageService.GetReadingStatisticsAsync();
            var challenges = await _storageService.GetActiveChallengesAsync();

            CurrentlyReading = new ObservableCollection<Book>(currentlyReading);
            LikedBooks = new ObservableCollection<Book>(liked);
            FinishedBooks = new ObservableCollection<Book>(finished);
            Statistics = stats;
            ActiveChallenge = challenges.FirstOrDefault();

            UpdateDisplayedBooks();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load library: {ex.Message}", "OK");
        }
    }

    public async Task RefreshLibraryAsync()
    {
        IsRefreshing = true;
        await LoadLibraryAsync();
        IsRefreshing = false;
    }

    private void UpdateDisplayedBooks()
    {
        DisplayedBooks = SelectedTab == "Liked"
            ? new ObservableCollection<Book>(LikedBooks)
            : new ObservableCollection<Book>(FinishedBooks);
    }

    private async Task ViewBookAsync(Book book)
    {
        if (book == null) return;
        await Shell.Current.GoToAsync($"//BookDetails?bookId={book.Id}");
    }

    private async Task ContinueReadingAsync(Book book)
    {
        if (book == null) return;
        // Navigate to reading page (to be implemented)
        await Shell.Current.GoToAsync($"//BookDetails?bookId={book.Id}");
    }
}