using Bonder.Models;
using System.Collections.ObjectModel;

namespace Bonder.ViewModels;

public class GenreSelectionViewModel : BaseViewModel
{
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

    public GenreSelectionViewModel()
    {
        // Initialize genres
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
            new Genre { Id = "children", Name = "Children", DisplayName = "Children", IsSelected = false },
            new Genre { Id = "nonfiction", Name = "Non-Fiction", DisplayName = "Non-Fiction", IsSelected = false },
            new Genre { Id = "horror", Name = "Horror", DisplayName = "Horror", IsSelected = false },
            new Genre { Id = "comedy", Name = "Comedy", DisplayName = "Comedy", IsSelected = false }
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

        // Update the genre in the collection to refresh UI
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
        // TODO: Save selected genres and navigate to main page
        await Shell.Current.GoToAsync("//MainPage");
    }

    private async Task SkipAsync()
    {
        // Navigate to main page without selecting genres
        await Shell.Current.GoToAsync("//MainPage");
    }
}