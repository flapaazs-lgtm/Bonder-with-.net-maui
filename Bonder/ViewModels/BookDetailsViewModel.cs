using Bonder.Models;
using Bonder.Services;
using System.Collections.ObjectModel;

namespace Bonder.ViewModels;

[QueryProperty(nameof(BookId), "bookId")]
public class BookDetailsViewModel : BaseViewModel
{
    private readonly IBookService _bookService;
    private readonly IRecommendationService _recommendationService;
    private readonly IStorageService _storageService;

    private Book _book;
    private string _bookId;
    private string _selectedTab = "About";
    private ObservableCollection<Book> _similarBooks = new();

    public Book Book
    {
        get => _book;
        set => SetProperty(ref _book, value);
    }

    public string BookId
    {
        get => _bookId;
        set
        {
            _bookId = value;
            _ = LoadBookDetailsAsync();
        }
    }

    public string SelectedTab
    {
        get => _selectedTab;
        set
        {
            SetProperty(ref _selectedTab, value);
            OnPropertyChanged(nameof(IsAboutTabSelected));
            OnPropertyChanged(nameof(IsReviewsTabSelected));
            OnPropertyChanged(nameof(IsSimilarTabSelected));
        }
    }

    public bool IsAboutTabSelected => SelectedTab == "About";
    public bool IsReviewsTabSelected => SelectedTab == "Reviews";
    public bool IsSimilarTabSelected => SelectedTab == "Similar";

    public ObservableCollection<Book> SimilarBooks
    {
        get => _similarBooks;
        set => SetProperty(ref _similarBooks, value);
    }

    public Command BackCommand { get; }
    public Command BookmarkCommand { get; }
    public Command ShareCommand { get; }
    public Command<string> SelectTabCommand { get; }
    public Command BuyOnAmazonCommand { get; }
    public Command FindOnBookshopCommand { get; }
    public Command<Book> ViewSimilarBookCommand { get; }

    public BookDetailsViewModel(
        IBookService bookService,
        IRecommendationService recommendationService,
        IStorageService storageService)
    {
        _bookService = bookService;
        _recommendationService = recommendationService;
        _storageService = storageService;

        BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        BookmarkCommand = new Command(async () => await BookmarkBookAsync());
        ShareCommand = new Command(async () => await ShareBookAsync());
        SelectTabCommand = new Command<string>(tab => SelectedTab = tab);
        BuyOnAmazonCommand = new Command(async () => await BuyOnAmazonAsync());
        FindOnBookshopCommand = new Command(async () => await FindOnBookshopAsync());
        ViewSimilarBookCommand = new Command<Book>(async (book) => await ViewSimilarBookAsync(book));
    }

    private async Task LoadBookDetailsAsync()
    {
        if (string.IsNullOrEmpty(BookId))
            return;

        try
        {
            Book = await _bookService.GetBookDetailsAsync(BookId);

            if (Book != null && SelectedTab == "Similar")
            {
                await LoadSimilarBooksAsync();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load book details: {ex.Message}", "OK");
        }
    }

    private async Task LoadSimilarBooksAsync()
    {
        if (Book == null) return;

        try
        {
            var recommendations = await _recommendationService.GetRecommendationsAsync(12);
            SimilarBooks = new ObservableCollection<Book>(recommendations.Where(b => b.Id != Book.Id).Take(10));
        }
        catch { /* Silent fail */ }
    }

    private async Task BookmarkBookAsync()
    {
        if (Book == null) return;

        await _storageService.AddToSavedBooksAsync(Book);
        await Shell.Current.DisplayAlert("Saved", $"{Book.Title} has been added to your library!", "OK");
    }

    private async Task ShareBookAsync()
    {
        if (Book == null) return;

        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = "Share Book",
            Text = $"Check out this book: {Book.Title} by {Book.AuthorsDisplay}"
        });
    }

    private async Task BuyOnAmazonAsync()
    {
        if (Book == null) return;

        var searchQuery = Uri.EscapeDataString($"{Book.Title} {Book.AuthorsDisplay}");
        var url = $"https://www.amazon.com/s?k={searchQuery}";

        await Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
    }

    private async Task FindOnBookshopAsync()
    {
        if (Book == null) return;

        var searchQuery = Uri.EscapeDataString($"{Book.Title} {Book.AuthorsDisplay}");
        var url = $"https://bookshop.org/search?keywords={searchQuery}";

        await Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
    }

    private async Task ViewSimilarBookAsync(Book book)
    {
        if (book == null) return;
        await Shell.Current.GoToAsync($"//BookDetails?bookId={book.Id}");
    }
}