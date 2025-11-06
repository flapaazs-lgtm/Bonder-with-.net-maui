using Bonder.Models;
using Bonder.Services;
using System.Collections.ObjectModel;

namespace Bonder.ViewModels;

public class GenreSelectionViewModel : BaseViewModel
{
    private readonly IStorageService _storageService;
    private readonly ObservableCollection<Genre> _allGenres;
    private readonly ObservableCollection<Genre> _selectedGenres = new();
    private bool _canContinue = false;

    public ObservableCollection<Genre> AllGenres => _allGenres;
    public ObservableCollection<Genre> SelectedGenres => _selectedGenres;

    public bool CanContinue
    {
        get => _canContinue;
        set => SetProperty(ref _canContinue, value);
    }

    public Command<Genre> ToggleGenreCommand { get; }
    public Command ContinueCommand { get; }
    public Command SkipCommand { get; }

    public GenreSelectionViewModel(IStorageService storageService)
    {
        _storageService = storageService;

        _allGenres = new ObservableCollection<Genre>
        {
            new Genre { Id = "fantasy", Name = "Fantasy", DisplayName = "Fantasy", IsSelected = false },
            new Genre { Id = "mystery", Name = "Mystery", DisplayName = "Mystery", IsSelected = false },
            new Genre { Id = "romance", Name = "Romance", DisplayName = "Romance", IsSelected = false },
            new Genre { Id = "science_fiction", Name = "Science Fiction", DisplayName = "Sci-Fi", IsSelected = false },
            new Genre { Id = "thriller", Name = "Thriller", DisplayName = "Thriller", IsSelected = false },
            new Genre { Id = "biography", Name = "Biography", DisplayName = "Biography", IsSelected = false },
            new Genre { Id = "history", Name = "History", DisplayName = "History", IsSelected = false },
            new Genre { Id = "young_adult", Name = "Young Adult", DisplayName = "Young Adult", IsSelected = false },
            new Genre { Id = "nonfiction", Name = "Non-Fiction", DisplayName = "Non-Fiction", IsSelected = false },
            new Genre { Id = "horror", Name = "Horror", DisplayName = "Horror", IsSelected = false },
            new Genre { Id = "poetry", Name = "Poetry", DisplayName = "Poetry", IsSelected = false },
            new Genre { Id = "self_help", Name = "Self Help", DisplayName = "Self Help", IsSelected = false }
        };

        ToggleGenreCommand = new Command<Genre>(ToggleGenre);
        ContinueCommand = new Command(async () => await ContinueAsync(), () => CanContinue);
        SkipCommand = new Command(async () => await SkipAsync());
    }

    private void ToggleGenre(Genre genre)
    {
        if (genre == null) return;

        genre.IsSelected = !genre.IsSelected;

        if (genre.IsSelected)
        {
            if (!_selectedGenres.Contains(genre))
                _selectedGenres.Add(genre);
        }
        else
        {
            _selectedGenres.Remove(genre);
        }

        // Force UI update
        var index = _allGenres.IndexOf(genre);
        if (index >= 0)
        {
            _allGenres[index] = genre;
        }

        CanContinue = _selectedGenres.Count >= 3;
        ContinueCommand.ChangeCanExecute();
    }

    private async Task ContinueAsync()
    {
        var preferences = await _storageService.LoadUserPreferencesAsync();
        preferences.GenreWeights = _selectedGenres.ToDictionary(
            g => g.Name.ToLower(),
            g => 1.0
        );
        await _storageService.SaveUserPreferencesAsync(preferences);

        var selectedGenreIds = string.Join(",", _selectedGenres.Select(g => g.Id));
        await Shell.Current.GoToAsync($"//TopBooksSelection?genres={selectedGenreIds}");
    }

    private async Task SkipAsync()
    {
        await Shell.Current.GoToAsync("//SwipeDiscovery");
    }
}