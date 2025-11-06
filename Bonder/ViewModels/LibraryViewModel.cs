using Bonder.Models;
using Bonder.Services;
using System.Collections.ObjectModel;

namespace Bonder.ViewModels;

public class LibraryViewModel : BaseViewModel
{
    private readonly IStorageService _storageService;
    private ObservableCollection<Book> _currentlyReading = new();
    private ObservableCollection<Book> _likedBooks = new();
    private ObservableCollection<Book> _finishedBooks = new();

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

    public Command ViewBookCommand { get; }
    public Command SortCommand { get; }
    public Command NavigateToDiscoverCommand { get; }
    public Command NavigateToLibraryCommand { get; }
    public Command NavigateToSearchCommand { get; }
    public Command NavigateToProfileCommand { get; }

    public LibraryViewModel(IStorageService storageService)
    {
        _storageService = storageService;

        ViewBookCommand = new Command<Book>(async (book) => await ViewBookAsync(book));
        SortCommand = new Command(async () => await ShowSortOptionsAsync());

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

            CurrentlyReading = new ObservableCollection<Book>(currentlyReading);
            LikedBooks = new ObservableCollection<Book>(liked);
            FinishedBooks = new ObservableCollection<Book>(finished);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load library: {ex.Message}", "OK");
        }
    }

    private async Task ViewBookAsync(Book book)
    {
        if (book == null) return;
        await Shell.Current.GoToAsync($"//BookDetails?bookId={book.Id}");
    }

    private async Task ShowSortOptionsAsync()
    {
        var action = await Shell.Current.DisplayActionSheet(
            "Sort Library",
            "Cancel",
            null,
            "By Title",
            "By Author",
            "By Date Added");

        switch (action)
        {
            case "By Title":
                LikedBooks = new ObservableCollection<Book>(LikedBooks.OrderBy(b => b.Title));
                FinishedBooks = new ObservableCollection<Book>(FinishedBooks.OrderBy(b => b.Title));
                break;
            case "By Author":
                LikedBooks = new ObservableCollection<Book>(LikedBooks.OrderBy(b => b.AuthorsDisplay));
                FinishedBooks = new ObservableCollection<Book>(FinishedBooks.OrderBy(b => b.AuthorsDisplay));
                break;
            case "By Date Added":
                await LoadLibraryAsync();
                break;
        }
    }

    public async Task RefreshLibraryAsync()
    {
        await LoadLibraryAsync();
    }
}