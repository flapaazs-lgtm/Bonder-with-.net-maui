using Bonder.Models;
using Bonder.Services;
using System.Collections.ObjectModel;

namespace Bonder.ViewModels;

[QueryProperty(nameof(GenresParam), "genres")]
public class TopBooksSelectionViewModel : BaseViewModel
{
    private readonly IBookService _bookService;
    private readonly IRecommendationService _recommendationService;
    private readonly IStorageService _storageService;
    private ObservableCollection<Book> _books = new();
    private string _searchQuery = "";
    private bool _isLoading = false;
    private bool _canContinue = false;
    private List<string> _selectedGenres = new();
    private int _minSelections = 3;
    private int _maxSelections = 5;
    private string _genresParam;

    public string GenresParam
    {
        get => _genresParam;
        set
        {
            _genresParam = value;
            if (!string.IsNullOrEmpty(value))
            {
                _selectedGenres = value.Split(',').ToList();
                _ = LoadInitialBooksAsync();
            }
        }
    }

    public ObservableCollection<Book> Books
    {
        get => _books;
        set => SetProperty(ref _books, value);
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
            {
                SearchCommand.Execute(null);
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool CanContinue
    {
        get => _canContinue;
        set => SetProperty(ref _canContinue, value);
    }

    public string SelectionStatusText =>
        $"{SelectedBooks.Count}/{_maxSelections} selected (min {_minSelections})";

    public List<Book> SelectedBooks => Books.Where(b => b.IsSelected).ToList();

    public Command SearchCommand { get; }
    public Command<Book> ToggleBookCommand { get; }
    public Command ContinueCommand { get; }

    public TopBooksSelectionViewModel(
        IBookService bookService,
        IRecommendationService recommendationService,
        IStorageService storageService)
    {
        _bookService = bookService;
        _recommendationService = recommendationService;
        _storageService = storageService;

        SearchCommand = new Command(async () => await SearchBooksAsync());
        ToggleBookCommand = new Command<Book>(ToggleBook);
        ContinueCommand = new Command(async () => await ContinueAsync(), () => CanContinue);
    }

    private async Task LoadInitialBooksAsync()
    {
        IsLoading = true;
        try
        {
            var books = new List<Book>();

            if (_selectedGenres?.Any() == true)
            {
                // Get books from selected genres
                foreach (var genreId in _selectedGenres.Take(3))
                {
                    var genreName = GetGenreName(genreId);
                    var genreBooks = await _bookService.GetBooksByGenreAsync(genreName, 10);
                    books.AddRange(genreBooks);
                }
            }
            else
            {
                // Get popular books if no genres selected
                books.AddRange(await _bookService.SearchBooksAsync("bestseller fiction", 15));
                books.AddRange(await _bookService.SearchBooksAsync("popular nonfiction", 15));
            }

            Books = new ObservableCollection<Book>(books.DistinctBy(b => b.Id).Take(30));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load books: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SearchBooksAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            await LoadInitialBooksAsync();
            return;
        }

        IsLoading = true;
        try
        {
            var results = await _bookService.SearchBooksAsync(SearchQuery, 30);
            Books = new ObservableCollection<Book>(results);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Search failed: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ToggleBook(Book book)
    {
        if (book == null) return;

        var selectedCount = SelectedBooks.Count;

        if (book.IsSelected)
        {
            book.IsSelected = false;
        }
        else
        {
            if (selectedCount >= _maxSelections)
            {
                Shell.Current.DisplayAlert("Limit Reached",
                    $"You can select up to {_maxSelections} books.", "OK");
                return;
            }
            book.IsSelected = true;
        }

        OnPropertyChanged(nameof(SelectionStatusText));
        CanContinue = SelectedBooks.Count >= _minSelections;
        ContinueCommand.ChangeCanExecute();
    }

    private async Task ContinueAsync()
    {
        var selectedBooks = SelectedBooks;

        // Train recommendation engine with selected books and genres
        var genreNames = _selectedGenres.Select(GetGenreName).ToList();
        await _recommendationService.TrainUserPreferencesAsync(genreNames, selectedBooks);

        // Save selected books to library
        foreach (var book in selectedBooks)
        {
            await _storageService.AddToLikedBooksAsync(book);
        }

        // Navigate to swipe discovery
        await Shell.Current.GoToAsync("//SwipeDiscovery");
    }

    private string GetGenreName(string genreId)
    {
        return genreId switch
        {
            "fantasy" => "Fantasy",
            "mystery" => "Mystery",
            "romance" => "Romance",
            "science_fiction" => "Science Fiction",
            "thriller" => "Thriller",
            "biography" => "Biography",
            "history" => "History",
            "young_adult" => "Young Adult",
            "nonfiction" => "Non-Fiction",
            "horror" => "Horror",
            "poetry" => "Poetry",
            "self_help" => "Self Help",
            _ => genreId
        };
    }
}