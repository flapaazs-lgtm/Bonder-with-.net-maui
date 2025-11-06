using Bonder.Models;

namespace Bonder.Services;

public interface IRecommendationService
{
    Task TrainUserPreferencesAsync(List<string> genres, List<Book> likedBooks);
    Task<List<Book>> GetRecommendationsAsync(int count = 20);
    Task<double> CalculateBookScoreAsync(Book book);
    Task RecordUserActionAsync(Book book, UserAction action);
}

public class RecommendationService : IRecommendationService
{
    private readonly IBookService _bookService;
    private readonly IStorageService _storageService;

    // User preference weights
    private Dictionary<string, double> _genreWeights = new();
    private Dictionary<string, double> _authorWeights = new();
    private HashSet<string> _likedBookIds = new();
    private HashSet<string> _dislikedBookIds = new();

    // Recommendation parameters
    private const double GENRE_WEIGHT = 0.4;
    private const double AUTHOR_WEIGHT = 0.2;
    private const double POPULARITY_WEIGHT = 0.2;
    private const double NOVELTY_WEIGHT = 0.2;

    public RecommendationService(IBookService bookService, IStorageService storageService)
    {
        _bookService = bookService;
        _storageService = storageService;
        _ = LoadUserPreferencesAsync();
    }

    public async Task TrainUserPreferencesAsync(List<string> genres, List<Book> likedBooks)
    {
        // Initialize genre weights from user selection
        _genreWeights.Clear();
        foreach (var genre in genres ?? new())
        {
            _genreWeights[genre.ToLower()] = 1.0;
        }

        // Learn from liked books
        foreach (var book in likedBooks ?? new())
        {
            _likedBookIds.Add(book.Id);

            // Boost genre weights
            foreach (var genre in book.Genres ?? new())
            {
                var key = genre.ToLower();
                _genreWeights[key] = _genreWeights.GetValueOrDefault(key, 0) + 0.5;
            }

            // Boost author weights
            foreach (var author in book.Authors ?? new())
            {
                var key = author.ToLower();
                _authorWeights[key] = _authorWeights.GetValueOrDefault(key, 0) + 0.5;
            }
        }

        // Normalize weights
        NormalizeWeights(_genreWeights);
        NormalizeWeights(_authorWeights);

        await SaveUserPreferencesAsync();
    }

    public async Task<List<Book>> GetRecommendationsAsync(int count = 20)
    {
        var candidates = new List<Book>();
        var seenBookIds = new HashSet<string>(_likedBookIds.Union(_dislikedBookIds));

        // Get books from preferred genres
        foreach (var genre in _genreWeights.OrderByDescending(g => g.Value).Take(5))
        {
            var books = await _bookService.GetBooksByGenreAsync(genre.Key, 15);
            candidates.AddRange(books.Where(b => !seenBookIds.Contains(b.Id)));
        }

        // Get books from preferred authors
        foreach (var author in _authorWeights.OrderByDescending(a => a.Value).Take(3))
        {
            var books = await _bookService.SearchBooksAsync($"author:{author.Key}", 10);
            candidates.AddRange(books.Where(b => !seenBookIds.Contains(b.Id)));
        }

        // Add some exploration (popular books from new genres)
        var explorationBooks = await _bookService.SearchBooksAsync("bestseller", 10);
        candidates.AddRange(explorationBooks.Where(b => !seenBookIds.Contains(b.Id)));

        // Score and rank books
        var scoredBooks = new List<(Book book, double score)>();
        foreach (var book in candidates.DistinctBy(b => b.Id))
        {
            var score = await CalculateBookScoreAsync(book);
            scoredBooks.Add((book, score));
        }

        // Return top recommendations
        return scoredBooks
            .OrderByDescending(b => b.score)
            .Select(b => b.book)
            .Take(count)
            .ToList();
    }

    public async Task<double> CalculateBookScoreAsync(Book book)
    {
        await Task.CompletedTask;

        double score = 0;

        // Genre matching score
        double genreScore = 0;
        if (book.Genres?.Any() == true)
        {
            genreScore = book.Genres
                .Select(g => _genreWeights.GetValueOrDefault(g.ToLower(), 0))
                .DefaultIfEmpty(0)
                .Average();
        }
        score += genreScore * GENRE_WEIGHT;

        // Author matching score
        double authorScore = 0;
        if (book.Authors?.Any() == true)
        {
            authorScore = book.Authors
                .Select(a => _authorWeights.GetValueOrDefault(a.ToLower(), 0))
                .DefaultIfEmpty(0)
                .Max();
        }
        score += authorScore * AUTHOR_WEIGHT;

        // Popularity score (based on rating)
        double popularityScore = (book.Rating ?? 3.5) / 5.0;
        score += popularityScore * POPULARITY_WEIGHT;

        // Novelty score (prefer newer books slightly)
        double noveltyScore = 0.5;
        if (book.FirstPublishYear.HasValue)
        {
            var age = DateTime.Now.Year - book.FirstPublishYear.Value;
            noveltyScore = Math.Max(0, 1 - (age / 50.0)); // Decay over 50 years
        }
        score += noveltyScore * NOVELTY_WEIGHT;

        // Penalize already seen books
        if (_dislikedBookIds.Contains(book.Id))
            score *= 0.1;

        return score;
    }

    public async Task RecordUserActionAsync(Book book, UserAction action)
    {
        switch (action)
        {
            case UserAction.Like:
                _likedBookIds.Add(book.Id);

                // Boost genre and author weights
                foreach (var genre in book.Genres ?? new())
                {
                    var key = genre.ToLower();
                    _genreWeights[key] = _genreWeights.GetValueOrDefault(key, 0) + 0.3;
                }
                foreach (var author in book.Authors ?? new())
                {
                    var key = author.ToLower();
                    _authorWeights[key] = _authorWeights.GetValueOrDefault(key, 0) + 0.3;
                }
                break;

            case UserAction.Dislike:
                _dislikedBookIds.Add(book.Id);

                // Reduce genre weights slightly
                foreach (var genre in book.Genres ?? new())
                {
                    var key = genre.ToLower();
                    _genreWeights[key] = Math.Max(0, _genreWeights.GetValueOrDefault(key, 0) - 0.1);
                }
                break;

            case UserAction.SaveForLater:
                _likedBookIds.Add(book.Id);
                break;
        }

        NormalizeWeights(_genreWeights);
        NormalizeWeights(_authorWeights);

        await SaveUserPreferencesAsync();
    }

    private void NormalizeWeights(Dictionary<string, double> weights)
    {
        if (!weights.Any()) return;

        var max = weights.Values.Max();
        if (max > 0)
        {
            var keys = weights.Keys.ToList();
            foreach (var key in keys)
            {
                weights[key] /= max;
            }
        }
    }

    private async Task LoadUserPreferencesAsync()
    {
        try
        {
            var prefs = await _storageService.LoadUserPreferencesAsync();
            if (prefs != null)
            {
                _genreWeights = prefs.GenreWeights ?? new();
                _authorWeights = prefs.AuthorWeights ?? new();
                _likedBookIds = prefs.LikedBookIds?.ToHashSet() ?? new();
                _dislikedBookIds = prefs.DislikedBookIds?.ToHashSet() ?? new();
            }
        }
        catch { /* First time user */ }
    }

    private async Task SaveUserPreferencesAsync()
    {
        var prefs = new UserPreferences
        {
            GenreWeights = _genreWeights,
            AuthorWeights = _authorWeights,
            LikedBookIds = _likedBookIds.ToList(),
            DislikedBookIds = _dislikedBookIds.ToList(),
            LastUpdated = DateTime.UtcNow
        };

        await _storageService.SaveUserPreferencesAsync(prefs);
    }
}

public enum UserAction
{
    Like,
    Dislike,
    SaveForLater
}