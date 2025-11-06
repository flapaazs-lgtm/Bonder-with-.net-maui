using Bonder.Models;
using System.Text.Json;

namespace Bonder.Services;

public interface IStorageService
{
    Task<UserPreferences> LoadUserPreferencesAsync();
    Task SaveUserPreferencesAsync(UserPreferences preferences);
    Task<List<Book>> GetLikedBooksAsync();
    Task AddToLikedBooksAsync(Book book);
    Task RemoveFromLikedBooksAsync(string bookId);
    Task<List<Book>> GetSavedBooksAsync();
    Task AddToSavedBooksAsync(Book book);
    Task<List<Book>> GetCurrentlyReadingAsync();
    Task AddToCurrentlyReadingAsync(Book book, double progress);
    Task UpdateReadingProgressAsync(string bookId, double progress);
    Task<List<Book>> GetFinishedBooksAsync();
    Task MarkAsFinishedAsync(Book book);

    // New methods for enhanced features
    Task<User> LoadUserProfileAsync();
    Task SaveUserProfileAsync(User user);
    Task<List<BookNote>> GetBookNotesAsync(string bookId);
    Task SaveBookNoteAsync(BookNote note);
    Task DeleteBookNoteAsync(string noteId);
    Task<List<ReadingChallenge>> GetActiveChallengesAsync();
    Task SaveReadingChallengeAsync(ReadingChallenge challenge);
    Task UpdateChallengeProgressAsync(string challengeId);
    Task<ReadingStatistics> GetReadingStatisticsAsync();
    Task<List<BookReview>> GetBookReviewsAsync(string bookId);
    Task SaveBookReviewAsync(BookReview review);
}

public class EnhancedStorageService : IStorageService
{
    private const string PreferencesKey = "user_preferences";
    private const string UserProfileKey = "user_profile";
    private const string LikedBooksKey = "liked_books";
    private const string SavedBooksKey = "saved_books";
    private const string CurrentlyReadingKey = "currently_reading";
    private const string FinishedBooksKey = "finished_books";
    private const string BookNotesKey = "book_notes";
    private const string ReadingChallengesKey = "reading_challenges";
    private const string ReadingStatsKey = "reading_statistics";
    private const string BookReviewsKey = "book_reviews";

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    // User Profile
    public async Task<User> LoadUserProfileAsync()
    {
        await Task.CompletedTask;
        var json = Preferences.Default.Get(UserProfileKey, string.Empty);

        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<User>(json, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveUserProfileAsync(User user)
    {
        await Task.CompletedTask;
        var json = JsonSerializer.Serialize(user, _jsonOptions);
        Preferences.Default.Set(UserProfileKey, json);
    }

    // Book Notes
    public async Task<List<BookNote>> GetBookNotesAsync(string bookId)
    {
        var allNotes = await GetAllBookNotesAsync();
        return allNotes.Where(n => n.BookId == bookId).OrderByDescending(n => n.CreatedAt).ToList();
    }

    public async Task SaveBookNoteAsync(BookNote note)
    {
        var allNotes = await GetAllBookNotesAsync();

        var existingNote = allNotes.FirstOrDefault(n => n.Id == note.Id);
        if (existingNote != null)
        {
            allNotes.Remove(existingNote);
        }

        allNotes.Add(note);
        await SaveAllBookNotesAsync(allNotes);
    }

    public async Task DeleteBookNoteAsync(string noteId)
    {
        var allNotes = await GetAllBookNotesAsync();
        allNotes.RemoveAll(n => n.Id == noteId);
        await SaveAllBookNotesAsync(allNotes);
    }

    private async Task<List<BookNote>> GetAllBookNotesAsync()
    {
        await Task.CompletedTask;
        var json = Preferences.Default.Get(BookNotesKey, string.Empty);

        if (string.IsNullOrEmpty(json))
            return new List<BookNote>();

        try
        {
            return JsonSerializer.Deserialize<List<BookNote>>(json, _jsonOptions) ?? new List<BookNote>();
        }
        catch
        {
            return new List<BookNote>();
        }
    }

    private async Task SaveAllBookNotesAsync(List<BookNote> notes)
    {
        await Task.CompletedTask;
        var json = JsonSerializer.Serialize(notes, _jsonOptions);
        Preferences.Default.Set(BookNotesKey, json);
    }

    // Reading Challenges
    public async Task<List<ReadingChallenge>> GetActiveChallengesAsync()
    {
        await Task.CompletedTask;
        var json = Preferences.Default.Get(ReadingChallengesKey, string.Empty);

        if (string.IsNullOrEmpty(json))
            return new List<ReadingChallenge>();

        try
        {
            var challenges = JsonSerializer.Deserialize<List<ReadingChallenge>>(json, _jsonOptions) ?? new List<ReadingChallenge>();
            return challenges.Where(c => c.EndDate >= DateTime.UtcNow).ToList();
        }
        catch
        {
            return new List<ReadingChallenge>();
        }
    }

    public async Task SaveReadingChallengeAsync(ReadingChallenge challenge)
    {
        var json = Preferences.Default.Get(ReadingChallengesKey, string.Empty);
        var challenges = new List<ReadingChallenge>();

        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                challenges = JsonSerializer.Deserialize<List<ReadingChallenge>>(json, _jsonOptions) ?? new List<ReadingChallenge>();
            }
            catch { }
        }

        var existing = challenges.FirstOrDefault(c => c.Id == challenge.Id);
        if (existing != null)
        {
            challenges.Remove(existing);
        }

        challenges.Add(challenge);

        await Task.CompletedTask;
        var updatedJson = JsonSerializer.Serialize(challenges, _jsonOptions);
        Preferences.Default.Set(ReadingChallengesKey, updatedJson);
    }

    public async Task UpdateChallengeProgressAsync(string challengeId)
    {
        var challenges = await GetActiveChallengesAsync();
        var challenge = challenges.FirstOrDefault(c => c.Id == challengeId);

        if (challenge != null)
        {
            var finishedBooks = await GetFinishedBooksAsync();
            challenge.CurrentProgress = finishedBooks.Count(b =>
                b.FinishedDate >= challenge.StartDate &&
                b.FinishedDate <= challenge.EndDate);

            await SaveReadingChallengeAsync(challenge);
        }
    }

    // Reading Statistics
    public async Task<ReadingStatistics> GetReadingStatisticsAsync()
    {
        var stats = new ReadingStatistics();

        var currentlyReading = await GetCurrentlyReadingAsync();
        var finished = await GetFinishedBooksAsync();
        var liked = await GetLikedBooksAsync();

        stats.BooksCurrentlyReading = currentlyReading.Count;
        stats.BooksFinished = finished.Count;
        stats.BooksLiked = liked.Count;

        // Calculate this year's stats
        var thisYear = DateTime.UtcNow.Year;
        stats.BooksFinishedThisYear = finished.Count(b => b.FinishedDate?.Year == thisYear);

        // Calculate pages read (if available)
        stats.TotalPagesRead = finished.Where(b => b.PageCount.HasValue).Sum(b => b.PageCount.Value);

        // Calculate favorite genres
        var genreCounts = new Dictionary<string, int>();
        foreach (var book in finished.Concat(liked))
        {
            foreach (var genre in book.Genres ?? new List<string>())
            {
                genreCounts[genre] = genreCounts.GetValueOrDefault(genre, 0) + 1;
            }
        }
        stats.FavoriteGenres = genreCounts.OrderByDescending(g => g.Value).Take(5).Select(g => g.Key).ToList();

        // Calculate reading streak
        stats.CurrentStreak = CalculateReadingStreak(finished);

        return stats;
    }

    private int CalculateReadingStreak(List<Book> finishedBooks)
    {
        if (!finishedBooks.Any())
            return 0;

        var sortedBooks = finishedBooks.Where(b => b.FinishedDate.HasValue)
            .OrderByDescending(b => b.FinishedDate.Value)
            .ToList();

        if (!sortedBooks.Any())
            return 0;

        int streak = 1;
        var currentDate = sortedBooks.First().FinishedDate.Value.Date;

        foreach (var book in sortedBooks.Skip(1))
        {
            var bookDate = book.FinishedDate.Value.Date;
            var daysDiff = (currentDate - bookDate).Days;

            if (daysDiff <= 7)
            {
                streak++;
                currentDate = bookDate;
            }
            else
            {
                break;
            }
        }

        return streak;
    }

    // Book Reviews
    public async Task<List<BookReview>> GetBookReviewsAsync(string bookId)
    {
        var allReviews = await GetAllBookReviewsAsync();
        return allReviews.Where(r => r.BookId == bookId).OrderByDescending(r => r.CreatedAt).ToList();
    }

    public async Task SaveBookReviewAsync(BookReview review)
    {
        var allReviews = await GetAllBookReviewsAsync();

        var existingReview = allReviews.FirstOrDefault(r => r.Id == review.Id);
        if (existingReview != null)
        {
            allReviews.Remove(existingReview);
        }

        allReviews.Add(review);
        await SaveAllBookReviewsAsync(allReviews);
    }

    private async Task<List<BookReview>> GetAllBookReviewsAsync()
    {
        await Task.CompletedTask;
        var json = Preferences.Default.Get(BookReviewsKey, string.Empty);

        if (string.IsNullOrEmpty(json))
            return new List<BookReview>();

        try
        {
            return JsonSerializer.Deserialize<List<BookReview>>(json, _jsonOptions) ?? new List<BookReview>();
        }
        catch
        {
            return new List<BookReview>();
        }
    }

    private async Task SaveAllBookReviewsAsync(List<BookReview> reviews)
    {
        await Task.CompletedTask;
        var json = JsonSerializer.Serialize(reviews, _jsonOptions);
        Preferences.Default.Set(BookReviewsKey, json);
    }

    // Implement existing methods from the original StorageService here...
    public async Task<UserPreferences> LoadUserPreferencesAsync()
    {
        await Task.CompletedTask;
        var json = Preferences.Default.Get(PreferencesKey, string.Empty);
        if (string.IsNullOrEmpty(json)) return new UserPreferences();
        try { return JsonSerializer.Deserialize<UserPreferences>(json, _jsonOptions) ?? new UserPreferences(); }
        catch { return new UserPreferences(); }
    }

    public async Task SaveUserPreferencesAsync(UserPreferences preferences)
    {
        await Task.CompletedTask;
        var json = JsonSerializer.Serialize(preferences, _jsonOptions);
        Preferences.Default.Set(PreferencesKey, json);
    }

    public async Task<List<Book>> GetLikedBooksAsync() => await GetBooksFromStorageAsync(LikedBooksKey);
    public async Task AddToLikedBooksAsync(Book book) => await AddBookToStorageAsync(LikedBooksKey, book);
    public async Task RemoveFromLikedBooksAsync(string bookId) => await RemoveBookFromStorageAsync(LikedBooksKey, bookId);
    public async Task<List<Book>> GetSavedBooksAsync() => await GetBooksFromStorageAsync(SavedBooksKey);
    public async Task AddToSavedBooksAsync(Book book) => await AddBookToStorageAsync(SavedBooksKey, book);
    public async Task<List<Book>> GetCurrentlyReadingAsync() => await GetBooksFromStorageAsync(CurrentlyReadingKey);

    public async Task AddToCurrentlyReadingAsync(Book book, double progress)
    {
        book.ReadProgress = progress;
        await AddBookToStorageAsync(CurrentlyReadingKey, book);
    }

    public async Task UpdateReadingProgressAsync(string bookId, double progress)
    {
        var books = await GetCurrentlyReadingAsync();
        var book = books.FirstOrDefault(b => b.Id == bookId);
        if (book != null)
        {
            book.ReadProgress = progress;
            if (progress >= 1.0)
            {
                await RemoveBookFromStorageAsync(CurrentlyReadingKey, bookId);
                await MarkAsFinishedAsync(book);
            }
            else
            {
                await SaveBooksToStorageAsync(CurrentlyReadingKey, books);
            }
        }
    }

    public async Task<List<Book>> GetFinishedBooksAsync() => await GetBooksFromStorageAsync(FinishedBooksKey);

    public async Task MarkAsFinishedAsync(Book book)
    {
        book.ReadProgress = 1.0;
        book.FinishedDate = DateTime.UtcNow;
        await AddBookToStorageAsync(FinishedBooksKey, book);
        await RemoveBookFromStorageAsync(CurrentlyReadingKey, book.Id);

        // Update challenges
        var challenges = await GetActiveChallengesAsync();
        foreach (var challenge in challenges)
        {
            await UpdateChallengeProgressAsync(challenge.Id);
        }
    }

    private async Task<List<Book>> GetBooksFromStorageAsync(string key)
    {
        await Task.CompletedTask;
        var json = Preferences.Default.Get(key, string.Empty);
        if (string.IsNullOrEmpty(json)) return new List<Book>();
        try { return JsonSerializer.Deserialize<List<Book>>(json, _jsonOptions) ?? new List<Book>(); }
        catch { return new List<Book>(); }
    }

    private async Task AddBookToStorageAsync(string key, Book book)
    {
        var books = await GetBooksFromStorageAsync(key);
        books.RemoveAll(b => b.Id == book.Id);
        books.Insert(0, book);
        await SaveBooksToStorageAsync(key, books);
    }

    private async Task RemoveBookFromStorageAsync(string key, string bookId)
    {
        var books = await GetBooksFromStorageAsync(key);
        books.RemoveAll(b => b.Id == bookId);
        await SaveBooksToStorageAsync(key, books);
    }

    private async Task SaveBooksToStorageAsync(string key, List<Book> books)
    {
        await Task.CompletedTask;
        var json = JsonSerializer.Serialize(books, _jsonOptions);
        Preferences.Default.Set(key, json);
    }
}