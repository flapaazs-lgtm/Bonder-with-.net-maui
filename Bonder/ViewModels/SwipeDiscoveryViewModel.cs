using Bonder.Models;
using Bonder.Services;
using System.Collections.ObjectModel;

namespace Bonder.ViewModels;

public class SwipeDiscoveryViewModel : BaseViewModel
{
    private readonly IRecommendationService _recommendationService;
    private readonly IStorageService _storageService;
    private ObservableCollection<Book> _bookQueue = new();
    private Book _currentBook;
    private bool _isEmpty = false;
    private bool _isLoading = false;

    public ObservableCollection<Book> BookQueue
    {
        get => _bookQueue;
        set => SetProperty(ref _bookQueue, value);
    }

    public Book CurrentBook
    {
        get => _currentBook;
        set => SetProperty(ref _currentBook, value);
    }

    public bool IsEmpty
    {
        get => _isEmpty;
        set => SetProperty(ref _isEmpty, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public Command LikeCommand { get; }
    public Command DislikeCommand { get; }
    public Command SaveForLaterCommand { get; }
    public Command RefreshCommand { get; }
    public Command<Book> ViewDetailsCommand { get; }
    public Command SettingsCommand { get; }
    public Command NavigateToDiscoverCommand { get; }
    public Command NavigateToLibraryCommand { get; }
    public Command NavigateToSearchCommand { get; }
    public Command NavigateToProfileCommand { get; }

    public SwipeDiscoveryViewModel(
        IRecommendationService recommendationService,
        IStorageService storageService)
    {
        _recommendationService = recommendationService;
        _storageService = storageService;

        LikeCommand = new Command(async () => await HandleSwipeAsync(UserAction.Like));
        DislikeCommand = new Command(async () => await HandleSwipeAsync(UserAction.Dislike));
        SaveForLaterCommand = new Command(async () => await HandleSwipeAsync(UserAction.SaveForLater));
        RefreshCommand = new Command(async () => await LoadRecommendationsAsync());
        ViewDetailsCommand = new Command<Book>(async (book) => await ViewBookDetailsAsync(book));
        SettingsCommand = new Command(async () => await NavigateToSettingsAsync());

        NavigateToDiscoverCommand = new Command(async () => await Shell.Current.GoToAsync("//SwipeDiscovery"));
        NavigateToLibraryCommand = new Command(async () => await Shell.Current.GoToAsync("//Library"));
        NavigateToSearchCommand = new Command(async () => await Shell.Current.GoToAsync("//Search"));
        NavigateToProfileCommand = new Command(async () => await Shell.Current.GoToAsync("//Profile"));

        _ = LoadRecommendationsAsync();
    }

    private async Task LoadRecommendationsAsync()
    {
        IsLoading = true;
        try
        {
            var recommendations = await _recommendationService.GetRecommendationsAsync(20);

            BookQueue = new ObservableCollection<Book>(recommendations);
            CurrentBook = BookQueue.FirstOrDefault();
            IsEmpty = !BookQueue.Any();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load recommendations: {ex.Message}", "OK");
            IsEmpty = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task HandleSwipeAsync(UserAction action)
    {
        if (CurrentBook == null) return;

        // Record user action for recommendation engine
        await _recommendationService.RecordUserActionAsync(CurrentBook, action);

        // Save to appropriate list
        switch (action)
        {
            case UserAction.Like:
                await _storageService.AddToLikedBooksAsync(CurrentBook);
                break;
            case UserAction.SaveForLater:
                await _storageService.AddToSavedBooksAsync(CurrentBook);
                break;
        }

        // Move to next book
        BookQueue.Remove(CurrentBook);
        CurrentBook = BookQueue.FirstOrDefault();

        // Preload more books if running low
        if (BookQueue.Count < 5)
        {
            _ = PreloadMoreBooksAsync();
        }

        IsEmpty = !BookQueue.Any();
    }

    private async Task PreloadMoreBooksAsync()
    {
        try
        {
            var moreBooks = await _recommendationService.GetRecommendationsAsync(10);
            foreach (var book in moreBooks)
            {
                if (!BookQueue.Any(b => b.Id == book.Id))
                {
                    BookQueue.Add(book);
                }
            }
        }
        catch { /* Silent fail for background operation */ }
    }

    private async Task ViewBookDetailsAsync(Book book)
    {
        if (book == null) return;
        await Shell.Current.GoToAsync($"//BookDetails?bookId={book.Id}");
    }

    private async Task NavigateToSettingsAsync()
    {
        await Shell.Current.GoToAsync("//Settings");
    }

    public void OnCardSwiped(Book book, SwipeDirection direction)
    {
        var action = direction switch
        {
            SwipeDirection.Left => UserAction.Dislike,
            SwipeDirection.Right => UserAction.Like,
            SwipeDirection.Up => UserAction.SaveForLater,
            _ => UserAction.Dislike
        };

        _ = HandleSwipeAsync(action);
    }
}

public enum SwipeDirection
{
    Left,
    Right,
    Up,
    Down
}